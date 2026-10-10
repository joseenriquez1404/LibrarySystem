using LibrarySystem.Models.Enums;

namespace LibrarySystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public Roles Role { get; set; } = Roles.User;
    }
}
