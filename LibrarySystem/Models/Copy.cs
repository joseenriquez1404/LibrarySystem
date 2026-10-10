using LibrarySystem.Models.Enums;

namespace LibrarySystem.Models
{
    public class Copy
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Code { get; set; } = string.Empty;
        public CopyStatus Status { get; set; } = CopyStatus.Available;
        public bool Active { get; set; } = true;
    }
}
