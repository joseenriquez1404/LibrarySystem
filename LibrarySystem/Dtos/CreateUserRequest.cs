using System.ComponentModel.DataAnnotations;
using LibrarySystem.Models.Enums;

namespace LibrarySystem.Dtos
{
    public class CreateUserRequest
    {
        [Required, MaxLength(RegisterRequest.MaxNameLength)]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(RegisterRequest.MaxEmailLength)]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(RegisterRequest.MinPasswordLength), MaxLength(RegisterRequest.MaxPasswordLength)]
        public string Password { get; set; } = string.Empty;
        public Roles Role { get; set; } = Roles.User;
    }
}
