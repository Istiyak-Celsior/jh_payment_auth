using jh_payment_auth.Entity;
using jh_payment_auth.Models;

namespace jh_payment_auth.Services.Services
{
    /// <summary>
    /// This service handles user authentication, including validating user credentials and generating JWT tokens.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly Dictionary<string, string> _users = new()
        {
            { "admin", "admin123" },
            { "user", "user123" }
        };

        private readonly IConfiguration _config;
        private readonly ITokenManagement _tokenManagement;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpClientService _httpClientService;

        /// <summary>
        /// Constructor for AuthService.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tokenManagement"></param>
        /// <param name="httpClientService"></param>
        public AuthService(IConfiguration config, ITokenManagement tokenManagement, IHttpClientService httpClientService)
        {
            _config = config;
            _tokenManagement = tokenManagement;
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Validates the user credentials against a predefined list of users.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<User?> ValidateUser(LoginRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _httpClientService.PutAsync<LoginRequest, User>($"v1/perops/User/getuser", request);
        }

        /// <summary>
        /// Login method that validates user credentials and generates a JWT token upon successful authentication.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<ResponseModel> Login(LoginRequest request)
        {
            var user = await ValidateUser(request);
            if (user == null)
            {
                return ErrorResponseModel.Fail("Invalid username or password", "AUT001");
            }

            var validTo = _config["Jwt:ExpiryInSec"] ?? throw new ArgumentNullException("Jwt:expiry not found in configuration.");

            var jwtToken = _tokenManagement.GenerateJwtToken(request.Email);

            return ResponseModel.Ok(
                 new AuthResponse
                 {
                     Token = jwtToken,
                     Expiration = validTo,
                     UserDetail = user
                 },
                 "Success"
            );
        }
    }
}
