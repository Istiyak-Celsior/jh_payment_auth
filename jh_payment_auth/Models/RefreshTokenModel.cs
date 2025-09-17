using System.ComponentModel.DataAnnotations;

namespace jh_payment_auth.Models
{
    /// <summary>
    /// Represents a model for managing refresh tokens, including their associated user, expiration, and revocation
    /// status.
    /// </summary>
    /// <remarks>This model is typically used in authentication workflows to handle refresh tokens, which
    /// allow clients to obtain new access tokens without re-authenticating.</remarks>
    public class RefreshTokenModel
    {
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool? IsRevoked { get; set; }
    }

    /// <summary>
    /// Represents the result of a token refresh operation, including its success status, any error details, and the
    /// refreshed tokens.
    /// </summary>
    /// <remarks>This class provides information about the outcome of a token refresh attempt.  If the
    /// operation is successful, the refreshed access token, refresh token, and their expiry details are populated.  If
    /// the operation fails, the <see cref="Error"/> property contains details about the failure.</remarks>
    public class RefreshTokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
