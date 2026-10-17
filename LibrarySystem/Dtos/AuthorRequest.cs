using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public class AuthorRequest
    {
        public const int MaxNameLength = 150;

        [Required, MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;
    }
}
