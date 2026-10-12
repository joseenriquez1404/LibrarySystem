using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public class RegisterRequest
    {
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 255;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;

        [Required, MaxLength(MaxNameLength)]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(MaxEmailLength)]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(MinPasswordLength), MaxLength(MaxPasswordLength)]
        public string Password { get; set; } = string.Empty;
    }
}
