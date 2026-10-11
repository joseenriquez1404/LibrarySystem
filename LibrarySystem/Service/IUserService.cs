using LibrarySystem.Dtos;
using LibrarySystem.Models.Enums;

namespace LibrarySystem.Service
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(int id);
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> ChangeRoleAsync(int actorId, int userId, Roles newRole);
        Task<UserDto> SetActiveAsync(int actorId, int userId, bool active);
    }
}
