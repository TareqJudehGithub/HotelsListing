using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

using HotelListingAPI.Common.Constants;
using HotelListingAPI.Domain;
using HotelListingAPI.Handlers;
using HotelListingAPI.Common.Models;
using HotelListingAPI.Application.Services;
using HotelListingAPI.Application.Contracts;
using HotelListingAPI.Application.MappingProfiles;


var builder = WebApplication.CreateBuilder(args);

// Add services to the IoC container.

// MSSQL Server Connection string
var connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
builder.Services.AddDbContext<HotelListingsDbContext>(options =>
options.UseSqlServer(connectionString));

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
    throw new InvalidOperationException("JwtSettings: Key is not configured.");
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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Add Identity endpoints middleware

// app.MapIdentityApi<ApplicationUser>();   for default endpoints path

// identity - AddIdentityEndPoints 
app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

// identity - custom authentication endpoints

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



