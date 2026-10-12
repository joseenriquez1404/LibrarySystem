using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class AuthService(AppDbContext _context, IPasswordHasher<User> _hasher, ITokenService _tokenService) : IAuthService
    {
        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var name = request.Name.Trim();
            var email = NormalizeEmail(request.Email);

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new ConflictException("A user with that email already exists.");
            }

            // Todo registro público crea un lector; solo un admin puede cambiar el rol.
            var user = new User { Name = name, Email = email, Role = Roles.User };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Dos registros simultáneos con el mismo email: gana el índice único.
                throw new ConflictException("A user with that email already exists.");
            }

            return user;
        }

        public async Task<(User User, string Token, DateTime ExpiresAt)> LoginAsync(LoginRequest request)
        {
            var email = NormalizeEmail(request.Email);
            var password = request.Password;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Mismo error si no existe, está desactivado o la contraseña es incorrecta.
            if (user is null || !user.Active)
            {
                throw new InvalidCredentialsException();
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException();
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(user, password);
                await _context.SaveChangesAsync();
            }

            var (token, expiresAt) = _tokenService.CreateToken(user);

            return (user, token, expiresAt);
        }

        private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}
