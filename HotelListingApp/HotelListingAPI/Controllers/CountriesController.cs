
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingAPI.Data;

namespace HotelListingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly HotelListingsDbContext _context;

    public CountriesController(HotelListingsDbContext context)
    {
        _context = context;
    }

    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
    {
        var countries = await _context.Countries
            .Include(q => q.Hotels)
            .ToListAsync();

        // If Countries entity has no records yet
        if (!_context.Countries.Any())
        {
            return Ok(new { message = "Countries has no records." });
        }

        return countries;
    }

    // GET: api/Countries/5
    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Country>> GetCountry([FromRoute] int id)
    {
        var country = await _context.Countries
            .Include(q => q.Hotels)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (!_context.Countries.Any())
        {
            return Ok(new { message = "Countries has no records." });
        }
        if (country == null)
        {
            return NotFound(new { message = $"Country with Id {id} was not found!" });
        }

        return country;
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> PutCountry([FromRoute] int id, [FromBody] Country country)
    {
        if (!await CountryExistsAsync(id))
        {
            return NotFound(new { message = "Invalid Id provided" });
        }

        _context.Entry(country).State = EntityState.Modified;

        // alt approach
        //_context.Countries.Update(country);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExistsAsync(id))
            {
                return NotFound(new { message = "Invalid Id provided" });
            }
            else
            {
                throw;
            }
        }
        return Ok(country);

        //  return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry([FromBody] Country country)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteCountry([FromRoute] int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> CountryExistsAsync(int id)
    {
        return await _context.Countries.AnyAsync(e => e.Id == id);
    }
}
