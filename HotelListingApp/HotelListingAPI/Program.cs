using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    //  Avoid errors from object cycles, and to return Country details in GetHotels endpoint.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// MSSQL Server Connection string
var connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
builder.Services.AddDbContext<HotelListingsDbContext>(options =>
options.UseSqlServer(connectionString));

// Adding Service Layer
builder.Services.AddScoped<ICountriesServices, CountriesService>();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
