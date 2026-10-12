using LibrarySystem.Dtos;
using LibrarySystem.Models;

namespace LibrarySystem.Service
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<(User User, string Token, DateTime ExpiresAt)> LoginAsync(LoginRequest request);
    }
}
