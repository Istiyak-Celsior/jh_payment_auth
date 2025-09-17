using jh_payment_auth.Models;
using jh_payment_auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jh_payment_auth.Controllers
{
    /// <summary>
    /// This controller handles authentication-related operations such as user login and accessing secure data.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="authService"></param>
    /// <param name="tokenManagementService"></param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth-service/[Controller]")]
    public class LoginController(IAuthService authService, ITokenManagement tokenManagementService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ITokenManagement _tokenManagementService = tokenManagementService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request);
            if (result == null) return Unauthorized("Invalid username or password");
            return Ok(result);
        }

        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <remarks>This method validates the provided refresh token and, if valid, generates a new
        /// access token and refresh token pair. Ensure that the <paramref name="request"/> contains a valid refresh
        /// token.</remarks>
        /// <param name="request">The request containing the refresh token to be used for generating a new access token.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the token refresh operation: <list type="bullet">
        /// <item> <description><see cref="OkObjectResult"/> with the new access token, refresh token, and expiration
        /// time if the operation succeeds.</description> </item> <item> <description><see
        /// cref="BadRequestObjectResult"/> if the refresh token is missing or invalid.</description> </item> <item>
        /// <description><see cref="UnauthorizedObjectResult"/> if the refresh operation fails due to invalid or expired
        /// tokens.</description> </item> </list></returns>
        [HttpPost("refreshtoken")]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var result = _tokenManagementService.RefreshAccessToken(request);

            if (!result.Success)
            {
                return Unauthorized(new RefreshTokenResult { Error = result.Error });
            }

            return Ok(new RefreshTokenResult
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                RefreshTokenExpiryDate = result.RefreshTokenExpiryDate,
                Success = result.Success
            });
        }

        /// <summary>
        /// This endpoint returns secure data and requires the user to be authenticated.
        /// </summary>
        /// <returns></returns>
        [HttpGet("secure-data")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            var username = User.Identity?.Name;
            return Ok(new { Message = $"Hello {username}, you are authenticated!" });
        }
    }
}
