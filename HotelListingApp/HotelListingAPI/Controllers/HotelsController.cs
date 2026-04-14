using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingAPI.Data;

namespace HotelListingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly HotelListingsDbContext _context;

    public HotelsController(HotelListingsDbContext context)
    {
        _context = context;
    }

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
    {
        var hotels = await _context.Hotels
            //  .Include(h => h.Country)   // Include country navigation property
            .ToListAsync();

        if (!hotels.Any())
        {
            return NotFound(new { message = "Hotels list is empty." });
        }
        return hotels;
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Hotel>> GetHotel(int id)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Country)     // Country details navigation propery that matches the foreign key
            .FirstOrDefaultAsync(q => q.Id == id);

        if (hotel == null)
        {
            return NotFound(new { message = $"Hotel with Id {id} was not found!" });
        }

        return hotel;
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel([FromRoute] int id, [FromBody] Hotel hotel)
    {
        if (id != hotel.Id)
        {
            return BadRequest(new { message = "Hotel not found. No Id was provided" });
        }

        _context.Entry(hotel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HotelExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return Ok(hotel);
        // return NoContent();
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel([FromBody] Hotel hotel)
    {

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel([FromRoute] int id)
    {


        var hotel = await _context.Hotels
            .Include(h => h.Country)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (hotel == null)
        {
            return NotFound(new { message = $"Hotel with Id: {id} was not found!" });
        }

        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();

        var hotels = await _context.Hotels.ToListAsync();
        return Ok(hotels);
    }

    private bool HotelExists(int id)
    {
        return _context.Hotels.Any(e => e.Id == id);
    }
}
