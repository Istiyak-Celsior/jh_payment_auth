using jh_payment_auth.Entity;

namespace jh_payment_auth.Models
{
    /// <summary>
    /// The LoginRequest class represents a request to authenticate a user, containing the necessary
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Represents the username of the user attempting to log in.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Represents the password of the user attempting to log in.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// The AuthResponse class represents the response returned after a successful authentication,
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Represents the authentication token issued to the user upon successful login.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Represents the expiration time of the authentication token.
        /// </summary>
        public string Expiration { get; set; }

        /// <summary>
        /// User detail
        /// </summary>
        public User UserDetail { set; get; }
    }
}
