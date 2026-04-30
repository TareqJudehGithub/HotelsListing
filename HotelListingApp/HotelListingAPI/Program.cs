using Microsoft.EntityFrameworkCore;

using HotelListingAPI.Data;
using HotelListingAPI.Contracts;
using HotelListingAPI.Services;
using HotelListingAPI.MappingProfiles;
using Microsoft.AspNetCore.Identity;

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
    // Identity Database store location
    .AddEntityFrameworkStores<HotelListingsDbContext>();

// Register AddAuthorization service
builder.Services.AddAuthorization();

// Adding Service Layers <abstract, implementation>
builder.Services.AddScoped<ICountriesServices, CountriesService>();
builder.Services.AddScoped<IHotelsServices, HotelsServices>();
builder.Services.AddScoped<IUsersServices, UsersServices>();
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


