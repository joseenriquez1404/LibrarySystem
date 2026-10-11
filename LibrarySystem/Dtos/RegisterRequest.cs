using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public record RegisterRequest
    {
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 255;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(MaxEmailLength)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(MaxPasswordLength, MinimumLength = MinPasswordLength)]
        public string Password { get; init; } = string.Empty;
    }
}
