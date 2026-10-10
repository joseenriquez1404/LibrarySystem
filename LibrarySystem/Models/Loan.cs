using LibrarySystem.Models.Enums;

namespace LibrarySystem.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CopyId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int Renewals { get; set; } = 0;
        public LoanStatus Status { get; set; } = LoanStatus.Active;
    }
}
