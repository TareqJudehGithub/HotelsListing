namespace HotelListingAPI.Domain
{
    public class HotelAdmin
    {
        #region Properties
        public int Id { get; set; }

        // FKs
        public required int HotelId { get; set; }
        public required string UserId { get; set; }

        // Navigation Properties 
        public Hotel? Hotel { get; set; }
        public ApplicationUser? User { get; set; }
        #endregion
    }
}
