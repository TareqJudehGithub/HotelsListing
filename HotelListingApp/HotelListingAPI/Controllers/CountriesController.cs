using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelListingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : BaseApiController
{
    private readonly ICountriesServices _countriesServices;
    public CountriesController(ICountriesServices countriesServices)
    {
        _countriesServices = countriesServices;
    }

    // GET: api/Countries
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var result = await _countriesServices.GetCountriesAsync();
        return ToActionResult(result: result);
    }

    // GET: api/Countries/5
    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry([FromRoute] int id)
    {
        var result = await _countriesServices.GetCountryAsync(id);

        return ToActionResult(result: result);
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry([FromBody] CreateCountryDto countryDto)
    {
        var result = await _countriesServices.CreateCountryAsync(countryDto);

        // Check for any error
        if (!result.IsSuccess)
        {
            return MapErrorsToResponse(errors: result.Errors);
        }

        return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.CountryId }, result);
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> PutCountry([FromRoute] int id, [FromBody] UpdateCountryDto countryDto)
    {

        var result = await _countriesServices.UpdateCountryAsync(id, countryDto);

        return ToActionResult(result: result);
    }

    // DELETE: api/Countries/5
    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteCountry([FromRoute] int id)
    {
        var result = await _countriesServices.DeleteCountryAsync(id);
        return ToActionResult(result: result);

        #region Delete using .Remove()
        //var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

        //if (country == null)
        //{
        //    return NotFound();
        //}

        //_context.Countries.Remove(country);
        //await _context.SaveChangesAsync();

        // return NoContent();
        #endregion
    }

}


#region CountriesController before Result pattern
//using Microsoft.AspNetCore.Mvc;

//using HotelListingAPI.Data;
//using HotelListingAPI.DTOs.Country;
//using HotelListingAPI.Results;

//namespace HotelListingAPI.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//public class CountriesController : ControllerBase
//{
//    private readonly ICountriesServices _countriesServices;
//    public CountriesController(ICountriesServices countriesServices)
//    {
//        _countriesServices = countriesServices;
//    }

//    // GET: api/Countries
//    [HttpGet]
//    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
//    {
//        var countries = await _countriesServices.GetCountriesAsync();

//        return Ok(countries);
//    }

//    // GET: api/Countries/5
//    [HttpGet]
//    [Route("{id:int}")]
//    public async Task<ActionResult<GetCountryDto>> GetCountry([FromRoute] int id)
//    {
//        var countries = await _countriesServices.GetCountriesAsync();

//        if (countries.Value.Count() == 0)
//        {
//            return Ok(new { message = "Countries has no records." });
//        }

//        var countryDto = await _countriesServices.GetCountryAsync(id);

//        if (countryDto == null)
//        {
//            return NotFound(new { message = $"Country with Id {id} was not found!" });
//        }
//        return Ok(countryDto);

//    }

//    // POST: api/Countries
//    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//    [HttpPost]
//    public async Task<ActionResult<Country>> PostCountry([FromBody] CreateCountryDto countryDto)
//    {
//        var resultDto = await _countriesServices.CreateCountryAsync(countryDto);

//        return CreatedAtAction(nameof(GetCountry), new { id = resultDto }, resultDto);
//    }

//    // PUT: api/Countries/5
//    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
//    [HttpPut]
//    [Route("{id:int}")]
//    public async Task<IActionResult> PutCountry([FromRoute] int id, [FromBody] UpdateCountryDto countryDto)
//    {
//        if (id != countryDto.Id)
//        {
//            return BadRequest(new { message = $"Country Id: {id} was not found." });
//        }
//        await _countriesServices.UpdateCountryAsync(id, countryDto);

//        return Ok(countryDto);
//    }

//    // DELETE: api/Countries/5
//    [HttpDelete]
//    [Route("{id:int}")]
//    public async Task<IActionResult> DeleteCountry([FromRoute] int id)
//    {
//        var countries = await _countriesServices.GetCountriesAsync();
//        await _countriesServices.DeleteCountryAsync(id);

//        return Ok(countries);

//        #region Delete using .Remove()
//        //var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

//        //if (country == null)
//        //{
//        //    return NotFound();
//        //}

//        //_context.Countries.Remove(country);
//        //await _context.SaveChangesAsync();

//        // return NoContent();
//        #endregion
//    }

#endregion