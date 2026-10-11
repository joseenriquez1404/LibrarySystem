using System.Net.Mail;
using LibrarySystem.Data;
using LibrarySystem.Dtos;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, ITokenService tokenService) : IAuthService
    {
        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            var name = request.Name?.Trim() ?? string.Empty;
            var email = NormalizeEmail(request.Email);

            if (name.Length == 0)
            {
                throw new InvalidInputException("Name is required.");
            }

            if (!IsValidEmail(email))
            {
                throw new InvalidInputException("Email is not valid.");
            }

            if (request.Password is null || request.Password.Length < RegisterRequest.MinPasswordLength)
            {
                throw new InvalidInputException($"Password must be at least {RegisterRequest.MinPasswordLength} characters long.");
            }

            if (await db.Users.AnyAsync(u => u.Email == email))
            {
                throw new ConflictException("A user with that email already exists.");
            }

            // Todo registro público crea un lector; solo un admin puede cambiar el rol.
            var user = new User { Name = name, Email = email, Role = Roles.User };
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            db.Users.Add(user);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Dos registros simultáneos con el mismo email: gana el índice único.
                throw new ConflictException("A user with that email already exists.");
            }

            return UserDto.From(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var email = NormalizeEmail(request.Email);
            var password = request.Password ?? string.Empty;

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Mismo error si no existe, está desactivado o la contraseña es incorrecta.
            if (user is null || !user.Active)
            {
                throw new InvalidCredentialsException();
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException();
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = hasher.HashPassword(user, password);
                await db.SaveChangesAsync();
            }

            var (token, expiresAt) = tokenService.CreateToken(user);

            return new AuthResponse(token, expiresAt, UserDto.From(user));
        }

        private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        private static bool IsValidEmail(string email) =>
            MailAddress.TryCreate(email, out var address) && address.Address == email;
    }
}
