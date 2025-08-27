using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string userId, string email, string role)
        {
            var jwt = _configuration.GetSection("JwtSettings");

            // Učitaj podatke iz konfiguracije
            var key = jwt["Key"] ?? throw new Exception("❌ JWT Key nedostaje u konfiguraciji.");
            var issuer = jwt["Issuer"] ?? "TerminoApp";
            var audience = jwt["Audience"] ?? "TerminoAppUsers";

            var expiresInMinutes = 120;
            if (int.TryParse(jwt["ExpiryMinutes"], out var parsed)) expiresInMinutes = parsed;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role), // za [Authorize(Roles = "...")]
                new Claim(ClaimTypes.NameIdentifier, userId), // .NET middleware koristi ovo
                new Claim("nameid", userId) // HotChocolate očekuje i ovo
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string Generate(User user)
        {
            return GenerateToken(user.Id, user.Email, user.Role);
        }
    }
}