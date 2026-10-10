using LibrarySystem.Models.Enums;

namespace LibrarySystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime Date { get; set; }
        public int Position { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    }
}
