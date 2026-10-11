using LibrarySystem.Models;
using LibrarySystem.Models.Enums;

namespace LibrarySystem.Dtos
{
    // Nunca expone PasswordHash.
    public record UserDto(int Id, string Name, string Email, Roles Role, bool Active)
    {
        public static UserDto From(User user) => new(user.Id, user.Name, user.Email, user.Role, user.Active);
    }
}
