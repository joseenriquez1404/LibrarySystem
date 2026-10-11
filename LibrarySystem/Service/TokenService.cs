using System.Security.Claims;
using System.Text;
using LibrarySystem.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LibrarySystem.Service
{
    public class TokenService(IOptions<JwtSettings> options) : ITokenService
    {
        private readonly JwtSettings jwt = options.Value;

        public (string Token, DateTime ExpiresAt) CreateToken(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = expiresAt,
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
        }
    }
}
