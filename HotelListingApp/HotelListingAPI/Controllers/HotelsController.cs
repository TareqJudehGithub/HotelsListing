using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Hotel;

namespace HotelListingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly IHotelsServices _hotelsServices;
    private readonly IMapper _mapper;
    //private readonly HotelListingsDbContext _context;

    public HotelsController(
        IHotelsServices hotelsServices,
        IMapper mapper)
    {
        _hotelsServices = hotelsServices;
        _mapper = mapper;
    }

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelDto>>> GetHotels()
    {
        var hotelsDto = await _hotelsServices.GetHotelsAsync();

        if (!hotelsDto.Any())
        {
            return NotFound(new { message = "Hotels list is empty." });
        }
        return Ok(hotelsDto);

        #region previous code
        // Get Hotels list directly from Hotels entity.
        //var hotels = await _context.Hotels
        //    //  .Include(h => h.Country)   // Include country navigation property
        //    .ToListAsync();
        #endregion
    }

    //GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotelDto = await _hotelsServices.GetHotelAsync(id);

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

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel([FromBody] CreateHotelDto createHotelDto)
    {
        var resultDto = await _hotelsServices.CreateHotelAsync(createHotelDto);

        if (resultDto is null)
        {
            return BadRequest(new { message = "Error creating new hotel." });
        }
        return CreatedAtAction(nameof(GetHotel), new { id = resultDto.Id }, resultDto);
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel([FromRoute] int id, [FromBody] UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id || hotelDto is null)
        {
            return BadRequest(new { message = "Hotel not found. No Id was provided" });
        }

        await _hotelsServices.UpdateHotelAsync(id, hotelDto);

        return Ok(hotelDto);

        #region Code before Layer Service
        //// Mark the Hotel entity as modified
        //_context.Entry(hotel).State = EntityState.Modified;

        //try
        //{
        //    await _context.SaveChangesAsync();
        //}
        //catch (DbUpdateConcurrencyException)
        //{
        //    if (!HotelExists(id))
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        throw;
        //    }
        //}
        //return Ok(hotelDto);

        // return NoContent();
        #endregion
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel([FromRoute] int id)
    {
        await _hotelsServices.DeleteHotelAsync(id);

        var hotels = await _hotelsServices.GetHotelsAsync();
        return Ok(hotels);
    }
}
