// Ignore Spelling: Dto

using Asp.Versioning;
using HotelListingAPI.Application.DTOs.Country;
using HotelListingAPI.Common.Models.Filtering;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListingAPI.Controllers;

//[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]

//[Authorize]
[AllowAnonymous]
#region RateLimiting
// Remove comment to enable Rate Limiting on this controller
//[EnableRateLimiting("fixed")]
#endregion
public class CountriesController : BaseApiController
{
    private readonly ICountriesServices _countriesServices;
    public CountriesController(ICountriesServices countriesServices)
    {
        _countriesServices = countriesServices;
    }

    // GET: api/Countries

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries(
        [FromQuery] CountryFilterParameters filters)
    {
        var result = await _countriesServices.GetCountriesAsync(filters);
        return ToActionResult(result: result);
    }

    // GET: api/countries/{id}/hotels
    [HttpGet("{countryId:int}/hotels")]
    public async Task<ActionResult<GetCountryHotelsDto>> GetCountriesHotels
        ([FromRoute] int countryId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] CountryFilterParameters filters)
    {
        var result = await _countriesServices.GetCountriesHotelsAsync
            (countryId, paginationParameters, filters);
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
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PutCountry
        ([FromRoute] int id,
        [FromBody] UpdateCountryDto countryDto)
    {

        var result = await _countriesServices.UpdateCountryAsync(id, countryDto);

        return ToActionResult(result: result);
    }

    [HttpPatch]
    [Route("{id:int}")]
    [Authorize(Roles = "Administrator")]
    // PATCH: api/countries/5
    public async Task<IActionResult> PatchCountry
        ([FromRoute] int Id,
        [FromBody] JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        if (patchDoc is null)
        {
            return BadRequest(new { message = "Patch document is required." });
        }

        var result = await _countriesServices.PatchCountryAsync(Id, patchDoc);
        return ToActionResult(result: result);
    }

    // DELETE: api/Countries/5
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "Administrator")]
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