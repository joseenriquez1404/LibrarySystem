using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public class CategoryRequest
    {
        public const int MaxNameLength = 100;

        [Required, MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;
    }
}
