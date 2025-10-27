using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : IJwtDecoderService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateJwtToken(User user)
        {

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToDouble(jwtSettings["ExpiryDays"])),//AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateJwtTokenForRegistration(string role, string email)
        {

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email,email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string? GetRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                return null;
            }
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key
            };

            try
            {
                var principal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                return roleClaim?.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool VerifyEmailFromToken(string token, string enteredEmail)
        {
            var handler = new JwtSecurityTokenHandler();

            // 1. Basic token check
            if (!handler.CanReadToken(token))
            {
                return false;
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            // Ensure you handle potential null reference if 'Secret' isn't configured
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));

            // 2. Token Validation Parameters (Identical to the original function)
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key
            };

            try
            {
                // 3. Validate the token and get the claims principal
                var principal = handler.ValidateToken(token, tokenValidationParameters, out _);

                // 4. Extract the email claim
                // ClaimTypes.Email is the standard claim type for email
                var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                // 5. Compare the extracted email with the entered email
                if (emailClaim?.Value != null)
                {
                    // Use String.Equals for a safe and robust comparison
                    return emailClaim.Value.Equals(enteredEmail, StringComparison.OrdinalIgnoreCase);
                }

                // No email claim found in the token
                return false;
            }
            catch (Exception)
            {
                // If validation fails (e.g., token expired, invalid signature), return false
                return false;
            }
        }
    }
}
