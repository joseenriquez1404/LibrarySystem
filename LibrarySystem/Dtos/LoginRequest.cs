using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public record LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(RegisterRequest.MaxEmailLength)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(RegisterRequest.MaxPasswordLength)]
        public string Password { get; init; } = string.Empty;
    }
}
