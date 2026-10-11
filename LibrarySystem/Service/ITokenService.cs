using LibrarySystem.Models;

namespace LibrarySystem.Service
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(User user);
    }
}
