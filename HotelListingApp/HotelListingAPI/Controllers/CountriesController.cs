using HotelListingAPI.DTOs.Country;
using HotelListingAPI.DTOs.Hotel;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingAPI.Data;
using HotelListingAPI.Contracts;

namespace HotelListingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountriesServices _countriesServices;
    // private readonly HotelListingsDbContext _context;

    public CountriesController(ICountriesServices countriesServices)
    {
        _countriesServices = countriesServices;
        // _context = context;
    }

    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var countries = _countriesServices.GetCountriesAsync();

        return Ok(countries);

        // var countries = await _context.Countries
        //    .Include(q => q.Hotels)
        //    .ToListAsync();
        // return countries;
    }

    // GET: api/Countries/5
    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry([FromRoute] int id)
    {
        var countryDto = _countriesServices.GetCountryAsync(id);
        return Ok(countryDto);

        //var coutryDto = await _context.Countries
        //    .Where(q => q.Id == id)
        //    .Select(q => new GetCountryDto(
        //        Id: q.Id,
        //        Name: q.Name,
        //        ShortName: q.Name,
        //        Hotels: q.Hotels
        //        .Select(q => new GetHotelsDto(
        //            Id: q.Id,
        //            Name: q.Name,
        //            Address: q.Address,
        //            Rating: q.Rating,
        //            CountryId: q.CountryId)
        //            ).ToList()
        //        ))
        //    .FirstOrDefaultAsync();

        //var country = await _context.Countries
        //    .Include(q => q.Hotels)
        //    .FirstOrDefaultAsync(q => q.Id == id);

        //if (!_context.Countries.Any())
        //{
        //    return Ok(new { message = "Countries has no records." });
        //}

        //if (coutryDto == null)
        //{
        //    return NotFound(new { message = $"Country with Id {id} was not found!" });
        //}

    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> PutCountry([FromRoute] int id, [FromBody] UpdateCountryDto countryDto)
    {
        var resultDto = _countriesServices.UpdateCountryAsync(id, countryDto);

        return Ok(countryDto);

        //var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

        //if (country is null)
        //{
        //    return NotFound(new { message = $"Country with Id: {id} was not found" });
        //}
        //// Update properties
        //country.Name = countryDto.Name;
        //country.ShortName = countryDto.ShortName;

        //if (!await CountryExistsAsync(id))
        //{
        //    return NotFound(new { message = "Invalid Id provided" });
        //}
        //_context.Entry(country).State = EntityState.Modified;

        // alt approach
        //_context.Countries.Update(country);

        //try
        //{
        //    await _context.SaveChangesAsync();
        //}
        //catch (DbUpdateConcurrencyException)
        //{
        //    if (!await CountryExistsAsync(id))
        //    {
        //        return NotFound(new { message = "Invalid Id provided" });
        //    }
        //    else
        //    {
        //        throw;
        //    }
        //}

        //  return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry([FromBody] CreateCountryDto countryDto)
    {
        var resultDto = _countriesServices.CreateCountry(countryDto);

        //var country = new Country
        //{
        //    Name = countryDto.Name,
        //    ShortName = countryDto.ShortName
        //};

        //_context.Countries.Add(country);
        //await _context.SaveChangesAsync();

        //// Convert back to DTO
        //var resultDto = new GetCountryDto(
        //    Id: country.Id,
        //    Name: country.Name,
        //    ShortName: country.ShortName,
        //    Hotels: []
        //    );

        return CreatedAtAction(nameof(GetCountry), new { id = resultDto.Id }, resultDto);
    }

    // DELETE: api/Countries/5
    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteCountry([FromRoute] int id)
    {
        await _countriesServices.DeleteCountryAsync(id);
        return NoContent();
        //var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

        //if (country == null)
        //{
        //    return NotFound();
        //}

        //_context.Countries.Remove(country);
        //await _context.SaveChangesAsync();

        // return NoContent();
    }
    //private async Task<bool> CountryExistsAsync(int id)
    //{
    //    return await _context.Countries.AnyAsync(e => e.Id == id);
    //}


}
