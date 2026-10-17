using LibrarySystem.Dtos;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;

namespace LibrarySystem.Service
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<User> CreateAsync(int actorId, CreateUserRequest request);
        Task<User> ChangeRoleAsync(int actorId, int userId, Roles newRole);
        Task<User> SetActiveAsync(int actorId, int userId, bool active);
    }
}
