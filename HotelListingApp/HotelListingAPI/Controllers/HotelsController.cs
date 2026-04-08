using HotelListingAPI.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new ()
            {
                Id = 1, Name = "Hotel California", Address = "42 Sunset Blvd, Los Angeles, CA", Rating = 4.5
            },
            new ()
            {
                Id = 2, Name = "The Grand Budapest Hotel", Address = "1 Alpine Drive, Zubrowka", Rating = 4.8
            }
        };

        // GET: api/<HotelsController>
        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            return Ok(hotels);
        }

        // GET api/<HotelsController>/5
        [HttpGet]
        [Route("{id:int}")]
        public ActionResult<Hotel> Get([FromRoute] int id)
        {
            var hotel = hotels.FirstOrDefault(p => p.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }

        // POST api/<HotelsController>
        [HttpPost]
        public ActionResult<Hotel> Post([FromBody] Hotel hotel)
        {

            // Check if the hotel with the same ID already exists
            if (hotels.Any(q => q.Id == hotel.Id))
            {
                return BadRequest(new { message = $"Hotel with ID {hotel.Id} already exists." });
            }

            hotels.Add(hotel);

            return CreatedAtAction(nameof(Get), new { id = hotel.Id }, hotel);
        }

        // PUT api/<HotelsController>/5
        [HttpPut("{id}")]
        public ActionResult<Hotel> Put(int id, [FromBody] Hotel hotel)
        {
            var hotelToUpdate = hotels.FirstOrDefault(p => p.Id == hotel.Id);

            // Check if the hotel exists
            if (hotelToUpdate is null)
            {
                return NotFound();
            }
            // Update the hotel details
            hotelToUpdate.Name = hotel.Name;
            hotelToUpdate.Address = hotel.Address;
            hotelToUpdate.Rating = hotel.Rating;

            return NoContent();
        }

        // DELETE api/<HotelsController>/5
        [HttpDelete("{id}")]
        public ActionResult<Hotel> Delete([FromRoute] int id)
        {
            var hotelToDelete = hotels.FirstOrDefault(p => p.Id == id);
            if (hotelToDelete is null)
            {
                return NotFound(new { message = "Hotel not found!" });
            }

            hotels.Remove(hotelToDelete);

            return NoContent();
        }
    }
}
