using jh_payment_auth.Constants;
using jh_payment_auth.DTOs;
using jh_payment_auth.Helpers;
using jh_payment_auth.Models;
using jh_payment_auth.Services;
using jh_payment_auth.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace jh_payment_auth.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing user-related operations, such as registration.
    /// </summary>
    /// <remarks>This controller handles user-related requests and delegates the operations to the underlying
    /// user service. It is designed to process HTTP requests for user management, including creating new
    /// users.</remarks>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth-service/[Controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService registrationService,
            ILogger<UsersController> logger)
        {
            _userService = registrationService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user based on the provided registration details.
        /// </summary>
        /// <remarks>This method processes the user registration request by delegating the operation to
        /// the user service.  Ensure that the <paramref name="request"/> object contains all required fields and
        /// adheres to the expected format.</remarks>
        /// <param name="request">The user registration details, including required information such as username, password, and email.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the registration operation.  Returns a 201 Created
        /// response with a success message if the registration is successful,  or a 500 Internal Server Error response
        /// with an error message if the registration fails.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationRequest request)
        {
            ResponseModel apiResponse = new ResponseModel();
            try
            {
                _logger.LogInformation("Received user registration request for email: {Email}", request.Email);

                apiResponse = await _userService.RegisterUserAsync(request);

                if (apiResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("User registration failed: {Errors}");
                    apiResponse.Message = UserErrorMessages.UserRegistrationFailed;
                    return StatusCode(((int)apiResponse.StatusCode), apiResponse);

                }
                else
                {
                    _logger.LogInformation("User registration successful for email: {Email}", request.Email);
                    apiResponse.Message = UserErrorMessages.UserRegistrationSuccess;
                    return Ok(apiResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during user registration for email: {Email}", request.Email);
                apiResponse = ErrorResponseModel.InternalServerError(UserErrorMessages.ErrorOccurredWhileRegistringUser, UserErrorMessages.ErrorOccurredWhileRegistringUserCode);
                return StatusCode(((int)apiResponse.StatusCode), apiResponse);
            }
        }

        /// <summary>
        /// List user by page
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="searchString"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        [HttpGet("list-users/{pageSize}/{pageNumber}/{searchString}/{sortBy}")]
        public async Task<ResponseModel> ListUsers([FromRoute] int pageSize, [FromRoute] int pageNumber, [FromRoute] string searchString, [FromRoute] string sortBy)
        {
            return await _userService.ListUserAsync(pageSize, pageNumber, searchString, sortBy);
        }

        /// <summary>
        /// Load dashboard
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("load-dashboard/{userId}")]
        public async Task<ResponseModel> LoadDashboard([FromRoute] int userId)
        {
            return await _userService.LoadDashboardAsync(userId);
        }
    }
}
