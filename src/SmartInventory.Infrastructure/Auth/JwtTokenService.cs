using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Domain.Identity;
using SmartInventory.Domain.Interfaces;
using SmartInventory.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartInventory.Infrastructure.Auth
{
    public class JwtTokenService(IOptions<JwtSettings> opts) : IJwtTokenService
    {
        private readonly JwtSettings _s = opts.Value;

        public string GenerateToken(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _s.Issuer,
                audience: _s.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_s.ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
