using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERPSystem.Infrastructure.Identity
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public AuthResponse CreateToken(Guid userId, int companyId, string email, string[] roles, string[] permissions)
        {
            var keyString = _config["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is missing.");
            var issuer = _config["JWT:IssuerIP"] ?? throw new InvalidOperationException("JWT:IssuerIP is missing.");
            var audience = _config["JWT:AudienceIP"] ?? throw new InvalidOperationException("JWT:AudienceIP is missing.");

            var expireMinutesString = _config["JWT:ExpireMinutes"] ?? throw new InvalidOperationException("JWT:ExpireMinutes is missing.");
            if (!int.TryParse(expireMinutesString, out var expireMinutes) || expireMinutes <= 0)
                throw new InvalidOperationException("JWT:ExpireMinutes must be a positive integer.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                // Standard claims
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),

                // Custom claim for multi-tenancy scoping
                new Claim("companyId", companyId.ToString()),
            };

            // Role claims (so [Authorize(Roles="...")] works)
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Permission claims (for policy-based authorization)
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponse(
                AccessToken: tokenString,
                ExpiresAtUtc: expiresAtUtc,
                UserId: userId,
                CompanyId: companyId,
                Email: email,
                Roles: roles,
                Permissions: permissions
            );
        }
    }
}
