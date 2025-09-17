using jh_payment_auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace jh_payment_auth.Services
{
    /// <summary>
    /// Class that generates and manages JWT and Refresh tokens
    /// </summary>
    public class TokenManagementService : ITokenManagement
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _accessTokenExpiry;
        private readonly string _refreshTokenExpiryDays;

        /// <summary>
        /// constructor that initializes the TokenManagementService with configuration settings.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenManagementService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:SecretKey not found in configuration.");
            _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer not found in configuration.");
            _audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience not found in configuration.");
            _accessTokenExpiry = configuration["Jwt:AccessTokenExpiryInSec"] ?? throw new ArgumentNullException("Jwt:AccessTokenExpiryInSec key not found in configuration.");
            _refreshTokenExpiryDays = configuration["Jwt:RefreshTokenExpiryDays"] ?? throw new ArgumentNullException("Jwt:RefreshTokenExpiryDays not found in configuration.");
        }

        /// <summary>
        /// Generates JWT token
        /// </summary>
        /// <param name="username"></param>
        /// <returns>JWT token string</returns>
        public string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Convert seconds to minutes for expiry
            var accessTokenExpirySeconds = Convert.ToDouble(_accessTokenExpiry);
            var accessTokenExpiryMinutes = accessTokenExpirySeconds / 60;

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Creates a new refresh token for the specified user.
        /// </summary>
        /// <remarks>The generated refresh token is valid for 7 days from the time of creation. The token
        /// is not revoked by default.</remarks>
        /// <param name="userName">The username for which the refresh token is being created. Cannot be null or empty.</param>
        /// <returns>A <see cref="RefreshTokenModel"/> containing the generated refresh token and its expiration date.</returns>
        public RefreshTokenModel CreateRefreshToken(string userName)
        {
            var refreshToken = new RefreshTokenModel
            {
                RefreshToken = GenerateRefreshToken(),
                Username = userName,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_refreshTokenExpiryDays)),
                IsRevoked = false
            };

            return new RefreshTokenModel
            {
                RefreshToken = refreshToken.RefreshToken,
                ExpiryDate = refreshToken.ExpiryDate
            };
        }

        /// <summary>
        /// Refreshes the access token using the provided refresh token model.
        /// </summary>
        /// <remarks>This method validates the provided refresh token to ensure it is not expired and has
        /// not been revoked. If the token is valid, a new access token and refresh token are generated. The used
        /// refresh token is marked as revoked.</remarks>
        /// <param name="refreshTokenModel">The model containing the refresh token and associated metadata, such as the token's expiry date and the
        /// username.</param>
        /// <returns>A <see cref="RefreshTokenResult"/> indicating the outcome of the operation. If successful, the result
        /// contains a new access token, a new refresh token, and the expiry date of the new refresh token. If
        /// unsuccessful, the result contains an error message.</returns>
        public RefreshTokenResult RefreshAccessToken(RefreshTokenModel refreshTokenModel)
        {
            if (refreshTokenModel == null)
            {
                return new RefreshTokenResult { Success = false, Error = "Invalid refresh token" };
            }

            if (refreshTokenModel.ExpiryDate < DateTime.UtcNow)
            {
                // Token has expired, mark it as revoked
                refreshTokenModel.IsRevoked = true;
                //await _context.SaveChangesAsync();
                return new RefreshTokenResult
                {
                    Success = false,
                    Error = "Refresh token expired",
                    RefreshTokenExpiryDate = refreshTokenModel.ExpiryDate
                };
            }

            // Generate new access token
            var newAccessToken = GenerateJwtToken(refreshTokenModel.Username);

            // Optionally, you can rotate refresh tokens for better security
            // This is recommended as a security best practice
            var newRefreshTokenResponse = CreateRefreshToken(refreshTokenModel.Username);

            // Revoke the used refresh token
            refreshTokenModel.IsRevoked = true;

            return new RefreshTokenResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenResponse.RefreshToken,
                RefreshTokenExpiryDate = newRefreshTokenResponse.ExpiryDate
            };
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// </summary>
        /// <remarks>The generated token is a Base64-encoded string derived from 32 bytes of random data.
        /// This method uses a cryptographic random number generator to ensure the token's security.</remarks>
        /// <returns>A Base64-encoded string representing the generated refresh token.</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }




    }
}
