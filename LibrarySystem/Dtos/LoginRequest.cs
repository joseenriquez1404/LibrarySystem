using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Dtos
{
    public class LoginRequest
    {
        [Required, MaxLength(RegisterRequest.MaxEmailLength)]
        public string Email { get; set; } = string.Empty;
        [Required, MaxLength(RegisterRequest.MaxPasswordLength)]
        public string Password { get; set; } = string.Empty;
    }
}
