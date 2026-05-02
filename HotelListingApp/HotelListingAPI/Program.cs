using HotelListingAPI.Constants;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.Handlers;
using HotelListingAPI.MappingProfiles;
using HotelListingAPI.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Authentication 
builder.Services.AddAuthentication(options =>
{
    // Add scheme

    // JWT 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


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
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            // Encode signing key using SymetricSecurityKey
            IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.UTF8
            .GetBytes(builder.Configuration["JwtSettings:Key"])),
            // Token expiry extra time - Zero for no extra time
            ClockSkew = TimeSpan.Zero
        };
    })

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

// AutoMapper service
builder.Services.AddAutoMapper(cfg =>
{
    // Hotel
    cfg.AddProfile<HotelMappingProfile>();
    // Country
    cfg.AddProfile<CountryMappingProfile>();

});

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



