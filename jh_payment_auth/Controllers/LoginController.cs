using jh_payment_auth.Models;
using jh_payment_auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jh_payment_auth.Controllers
{
    /// <summary>
    /// This controller handles authentication-related operations such as user login and accessing secure data.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth-service/[Controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authService"></param>
        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public async Task<ResponseModel> Login([FromBody] LoginRequest request)
        {
            return await _authService.Login(request);
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
