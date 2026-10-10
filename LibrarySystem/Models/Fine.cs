using LibrarySystem.Models.Enums;

namespace LibrarySystem.Models
{
    public class Fine
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public FineStatus Status { get; set; } = FineStatus.Pending;
    }
}
