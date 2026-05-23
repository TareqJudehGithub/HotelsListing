using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;

using HotelListingAPI.Common.Constants;
using HotelListingAPI.Domain;
using HotelListingAPI.Handlers;
using HotelListingAPI.Application.Services;
using HotelListingAPI.Application.Contracts;
using HotelListingAPI.Application.MappingProfiles;
using HotelListingAPI.Common.Models.Config;
using HotelListing.Api.Middleware;

// Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HotelListing API");

    var builder = WebApplication.CreateBuilder(args);

    // Inject Serilog service
    builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
   );

    // Add services to the IoC container.

    // MSSQL Server Connection string
    var connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
    builder.Services.AddDbContext<HotelListingsDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            // Connection retries
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
                );
        });
        // For development only
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        // Setting AsNoTracking() globally
        //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });

    // Identity service
    #region Identity - injecting AddIdentityCore<>
    //builder.Services.AddIdentityCore<ApplicationUser>(options =>
    //{
    //    // Password requires an uppercase character
    //    options.Password.RequireUppercase = true;
    //    options.Password.RequiredUniqueChars = 1;
    //    options.Password.RequireDigit = true;
    //})
    //    // Roles
    //    .AddRoles<IdentityRole>()
    //    // Identity Database store location
    //    .AddEntityFrameworkStores<HotelListingsDbContext>();
    #endregion

    // Identity service - AddIdentityEndPoints<> - Access to API endpoints
    builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredUniqueChars = 1;
    }
    )
        // identity Roles
        .AddRoles<IdentityRole>()

        // Identity Database store location
        .AddEntityFrameworkStores<HotelListingsDbContext>();

    // HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    // Authentication 
    // Bind appsettings.json to JwtSettings model
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
    // Check for JWT key
    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        Log.Fatal("JwtSettings:Key is not configured");
        throw new InvalidOperationException("JwtSettings:Key is not configured.");
    }

    builder.Services.AddAuthentication(options =>
    {
        // Add scheme and set it as default - JWT 
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        // Basic Auth
        //options.DefaultAuthenticateScheme = AuthenticationDefaults.BasicScheme; 
        //options.DefaultChallengeScheme = AuthenticationDefaults.BasicScheme;
    })
        // Handle scheme

        // JWT 
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate:
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                // Validate against:
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                // Encode signing key using SymetricSecurityKey
                IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.UTF8
                .GetBytes(jwtSettings.Key)),
                // Token expiry extra time - Zero for no extra time
                ClockSkew = TimeSpan.Zero
            };
        })

        // JWT
        // Basic auth and API Key
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationDefaults.BasicScheme, _ => { })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AuthenticationDefaults.ApiKeyScheme, _ => { });

    // Register AddAuthorization service
    builder.Services.AddAuthorization();

    // Adding Service Layers <abstract, implementation>
    builder.Services.AddScoped<ICountriesServices, CountriesService>();
    builder.Services.AddScoped<IHotelsServices, HotelsServices>();
    builder.Services.AddScoped<IUsersServices, UsersServices>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
    builder.Services.AddScoped<IBookingService, BookingService>();

    // Exception Handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    #region AutoMapper
    // AutoMapper service
    builder.Services.AddAutoMapper(cfg =>
    {
        // Hotel
        cfg.AddProfile<HotelMappingProfile>();
        // Country
        cfg.AddProfile<CountryMappingProfile>();
        // Booking
        cfg.AddProfile<HotelBookingMappingProfile>();
    });
    // Or add all mapping profiles using GetExecutingAssembly method
    //builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
    #endregion

    //  Avoid errors from object cycles, and to return Country details in GetHotels endpoint.
    builder.Services.AddControllers()
        // Patch requirement (NewtonsoftJson)
        .AddNewtonsoftJson()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


    // Register  In-Memory Caching
    builder.Services.AddMemoryCache();

    // Out-Put Caching
    //builder.Services.AddOutputCache();

    // Rate Limiting service
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.Window = TimeSpan.FromSeconds(25);
            opt.PermitLimit = 1;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken: cancellationToken);
        };

    });

    var app = builder.Build();

    // Exception Handler middleware
    app.UseExceptionHandler();

    // Serilog middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

        options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400
                ? LogEventLevel.Warning
                : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
            }
        };
    });

    // Add Identity endpoints middleware
    // app.MapIdentityApi<ApplicationUser>();   for default endpoints path

    // identity - AddIdentityEndPoints 
    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

    // identity - custom authentication endpoints

    app.UseHttpsRedirection();

    // Rate Limiting
    app.UseRateLimiter();

    app.UseAuthorization();

    // Out-Put Cache middleware
    //app.UseOutputCache();

    app.MapControllers();

    Log.Information("HotelListing API started successfully!");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


