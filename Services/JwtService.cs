using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TerminoApp_NewBackend.Models; // ✅ Dodano zbog User modela

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

            // potrebne vrijednosti iz appsettings.json
            var key = jwt["Key"] ?? throw new Exception("JWT Key missing in configuration.");
            var issuer = jwt["Issuer"] ?? "TerminoApp";
            var audience = jwt["Audience"] ?? "TerminoAppUsers";

            // dozvoli da u appsettings dodaš "ExpiresInMinutes"; default 120 min
            var expiresInMinutes = 120;
            if (int.TryParse(jwt["ExpiresInMinutes"], out var parsed)) expiresInMinutes = parsed;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, userId), // .NET
                new Claim("nameid", userId) // HotChocolate očekuje ovo!
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

        // ✅ NOVO: metoda koja prima User objekt
        public string Generate(User user)
        {
            return GenerateToken(user.Id, user.Email, user.Role);
        }
    }
}