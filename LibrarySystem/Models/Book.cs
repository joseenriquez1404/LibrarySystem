namespace LibrarySystem.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Author> Authors { get; set; } = new List<Author>();
        public string ISBN { get; set; } = string.Empty;
        public int Year { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool Active { get; set; } = true;

    }
}
