using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Text;
using System.Text.Json;

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

#region Logging
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
#endregion

try
{
    Log.Information("Starting HotelListing API");

    var builder = WebApplication.CreateBuilder(args);

    #region Serilog service

    // Inject Serilog service
    builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
   );
    #endregion

    // Add services to the IoC container.

    #region MSSQL Server Connection string
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
    #endregion

    #region Identity services(s)
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
    #endregion

    // HttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    #region Authentication
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
    #endregion

    // Register AddAuthorization service
    builder.Services.AddAuthorization();

    #region DI services

    // Adding Service Layers <abstract, implementation>
    builder.Services.AddScoped<ICountriesServices, CountriesService>();
    builder.Services.AddScoped<IHotelsServices, HotelsServices>();
    builder.Services.AddScoped<IUsersServices, UsersServices>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
    builder.Services.AddScoped<IBookingService, BookingService>();
    #endregion

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

    #region Caching

    // Register  In-Memory Caching
    builder.Services.AddMemoryCache();

    // Out-Put Caching
    //builder.Services.AddOutputCache();
    #endregion

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

    #region HealthCheck Service

    builder.Services.AddHealthChecks()
        // Regular self-check
        .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"),
        tags: ["api"])
        // DB check
        .AddDbContextCheck<HotelListingsDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql"]
        );

    #region Health Check UI service
    //builder.Services.AddHealthChecksUI(setup =>
    //{
    //    setup.SetEvaluationTimeInSeconds(10); // Check every 10 seconds
    //    setup.MaximumHistoryEntriesPerEndpoint(50);
    //    setup.AddHealthCheckEndpoint("HotelListing API", "/healthz");
    //})
    //    .AddInMemoryStorage();
    #endregion

    #endregion

    var app = builder.Build();

    #region Middlewares

    // Exception Handler middleware
    app.UseExceptionHandler();

    #region Serilog middleware

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
    #endregion

    #region Identity Endpoints    
    // Add Identity endpoints middleware
    // app.MapIdentityApi<ApplicationUser>();   for default endpoints path

    // identity - AddIdentityEndPoints 
    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();
    #endregion

    app.UseHttpsRedirection();

    #region Health Check Middleware
    // Health Check
    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration,
                    exception = entry.Value.Exception?.Message,
                    data = entry.Value.Data
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    });

    app.MapHealthChecks("/healthz/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });
    // Readiness Check
    app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db")
    });

    #region HealthChecks UI delegates
    // HealthCheck UI
    //app.MapHealthChecks("/healthz-ui", new HealthCheckOptions
    //{
    //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    //});

    // HealthChecks UI
    //app.MapHealthChecksUI(options =>
    //{
    //    options.UIPath = "/healthchecks-ui";
    //    options.ApiPath = "/healthchecks-api";
    //});
    #endregion
    #endregion

    // Rate Limiting
    app.UseRateLimiter();

    app.UseAuthorization();

    // Out-Put Cache middleware
    //app.UseOutputCache();

    app.MapControllers();

    Log.Information("HotelListing API started successfully!");
    #endregion

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


