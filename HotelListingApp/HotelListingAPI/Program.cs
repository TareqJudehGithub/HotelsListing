using Microsoft.EntityFrameworkCore;

using HotelListingAPI.Data;
using HotelListingAPI.Contracts;
using HotelListingAPI.Services;
using HotelListingAPI.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the IoC container.

// MSSQL Server Connection string
var connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
builder.Services.AddDbContext<HotelListingsDbContext>(options =>
options.UseSqlServer(connectionString));

// Adding Service Layer for CountriesService and HotelsServices
builder.Services.AddScoped<ICountriesServices, CountriesService>();
builder.Services.AddScoped<IHotelsServices, HotelsServices>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
