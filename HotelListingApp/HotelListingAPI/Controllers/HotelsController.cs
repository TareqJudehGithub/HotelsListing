using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;
using HotelListingAPI.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Net;

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
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotels()
    {
        var hotels = await _context.Hotels
            .Select(h => new GetHotelsDto(
                h.Id,
                h.Name,
                Address: h.Address,
                Rating: h.Rating,
                CountryId: h.CountryId
                ))
            .ToListAsync();

        if (!hotels.Any())
        {
            return NotFound(new { message = "Hotels list is empty." });
        }
        return Ok(hotels);

        #region previous code
        // Get Hotels list directly from Hotels entity.
        //var hotels = await _context.Hotels
        //    //  .Include(h => h.Country)   // Include country navigation property
        //    .ToListAsync();
        #endregion
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotelDto = await _context.Hotels
            .Where(q => q.Id == id)
            .Select(q => new GetHotelDto(
                Id: q.Id,
                Name: q.Name,
                Address: q.Address,
                Rating: q.Rating,
                Country: q.Country!.Name
                ))
            .FirstOrDefaultAsync();
        if (hotelDto is null)
        {
            return NotFound(new { message = $"Hotel with Id: {id} was not found." });
        }
        return Ok(hotelDto);

        // Or
        //var hotel = await _context.Hotels
        //    .Include(h => h.Country) // Country details navigation propery that matches the foreign key
        //    .FirstOrDefaultAsync(h => h.Id == id);

        //if (hotel == null)
        //{
        //    return NotFound(new { message = $"Hotel with Id: {id} was not found." });
        //}
        //;

        //var hotelDto = new GetHotelDto(
        //    hotel.Id,
        //    hotel.Name,
        //    hotel.Address,
        //    hotel.Rating,
        //    hotel.Country!.Name // include Country (name only)
        //);

        //return hotelDto;
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel([FromRoute] int id, [FromBody] UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest(new { message = "Hotel not found. No Id was provided" });
        }

        // Get target hotel 
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);

        if (hotel is null)
        {
            return NotFound(new { message = "Hotel was not found" });
        }

        // Update records
        hotel.Name = hotelDto.Name;
        hotel.Address = hotelDto.Address;
        hotel.Rating = hotelDto.Rating;
        hotel.CountryId = hotelDto.CountryId;

        // Mark the Hotel entity as modified
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
        return Ok(hotelDto);

        // return NoContent();
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel([FromBody] CreateHotelDto hotelDto)
    {
        var hotel = new Hotel
        {
            Name = hotelDto.Name,
            Address = hotelDto.Address,
            Rating = hotelDto.Rating,
            CountryId = hotelDto.CountryId
        };

        // Add new hotel object created into DB and save.
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        var resultDto = new CreateHotelDto
        {
            Name = hotel.Name,
            Address = hotel.Address,
            Rating = hotel.Rating,
            CountryId = hotel.CountryId
        };

        if (resultDto is null)
        {
            return BadRequest(new { message = "Error creating new hotel." });
        }

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, resultDto);
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

    // Check if hotel exists in DB
    private bool HotelExists(int id)
    {
        return _context.Hotels.Any(e => e.Id == id);
    }
}
