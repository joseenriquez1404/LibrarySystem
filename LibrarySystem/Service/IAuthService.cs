using LibrarySystem.Dtos;

namespace LibrarySystem.Service
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
