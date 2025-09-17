using jh_payment_auth.Constants;
using jh_payment_auth.DTOs;
using jh_payment_auth.Entity;
using jh_payment_auth.Helpers;
using jh_payment_auth.Models;
using jh_payment_auth.Validators;
using Microsoft.AspNetCore.Identity;

namespace jh_payment_auth.Services
{
    /// <summary>
    /// Implements user-related operations, including registration, by interacting with the user repository and validation services.
    /// </summary>
    public class UsersService : IUserService
    {
        private readonly ILogger<UsersService> _logger;
        private readonly IValidationService _validationService;
        private readonly IHttpClientService _httpClientService;

        public UsersService(ILogger<UsersService> logger,
            IValidationService validationService, IHttpClientService httpClientService)
        {
            _logger = logger;
            _validationService = validationService;
            _httpClientService = httpClientService;
        }

        // Pseudocode:
        // 1. After checking for existing user by UserId, also check for existing user by Email and AccountNumber.
        // 2. If a user exists with the same Email or AccountNumber, return a duplicate error response.
        // 3. Add new private methods to check for user existence by Email and AccountNumber using IHttpClientService.
        // 4. Integrate these checks into RegisterUserAsync before creating the new user.

        // Implementation:
        public async Task<ResponseModel> RegisterUserAsync(UserRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to register new user with email: {Email}", request.Email);
                _logger.LogInformation("Attempting to register new user with Account Number: {AccountNumber}", request.AccountDetails.AccountNumber);

                // Step 1: Validate the incoming request data, including new fields.
                var validationErrors = _validationService.ValidateRegistrationRequest(request);
                if (validationErrors.Count > 0)
                {
                    _logger.LogError("User registration validation failed: {Errors}", string.Join(", ", validationErrors));
                    return ErrorResponseModel.BadRequest(UserErrorMessages.UserValidationFailed + " Validation Errors: \n" + string.Join(", ", validationErrors), UserErrorMessages.UserValidationFailedCode);
                }

                // Step 2: Check for existing user by UserId.
                var user = await GetUserData(request.UserId);
                if (user != null)
                {
                    _logger.LogError("Registration failed: User with the id {UserId} already exists.", request.UserId);
                    return ErrorResponseModel.BadRequest(UserErrorMessages.UserAccountAlreadyExists, UserErrorMessages.UserAlreadyExistsCode);
                }

                // Step 2a: Check for existing user by Email.
                var userByEmail = await GetUserByEmail(request.Email);
                if (userByEmail != null)
                {
                    _logger.LogError("Registration failed: User with the email {Email} already exists.", request.Email);
                    return ErrorResponseModel.BadRequest(UserErrorMessages.UserAccountAlreadyExists, UserErrorMessages.UserAlreadyExistsCode);
                }

                // Step 2b: Check for existing user by AccountNumber.
                var userByAccountNumber = await GetUserByAccountNumber(request.AccountDetails.AccountNumber);
                if (userByAccountNumber != null)
                {
                    _logger.LogError("Registration failed: User with the account number {AccountNumber} already exists.", request.AccountDetails.AccountNumber);
                    return ErrorResponseModel.BadRequest(UserErrorMessages.UserAccountAlreadyExists, UserErrorMessages.UserAlreadyExistsCode);
                }

                // Step 3: Hash the password for security.
                var hashedPassword = Utility.HashPassword(request.Password);

                // Step 4: Create the new user model, mapping all fields from the request.
                var newUser = new User
                {
                    UserId = request.UserId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = hashedPassword,
                    Age = request.Age,
                    Mobile = request.PhoneNumber,
                    Address = request.Address.Street + ", " + request.Address.City,
                    AccountNumber = request.AccountDetails.AccountNumber,
                    BankName = request.AccountDetails.BankName,
                    IFCCode = request.AccountDetails.IFSCCode,
                    Branch = request.AccountDetails.Branch,
                    IsActive = true,
                    CVV = request.AccountDetails.CVV,
                    DateOfExpiry = request.AccountDetails.DateOfExpiry,
                    UPIID = request.AccountDetails.UPIId,
                };

                // Step 5: Persist the user data.
                var response = await AddUserData(newUser);
                if (response == null)
                {
                    _logger.LogError("User registration failed for email: {Email} and Account Number: {AccountNumber}", request.Email, request.AccountDetails.AccountNumber);
                    return ErrorResponseModel.InternalServerError(UserErrorMessages.UserRegistrationFailed, UserErrorMessages.UserRegistrationFailedCode);
                }

                _logger.LogInformation("User with email: {Email} and Account Number: {AccountNumber} registered successfully.", request.Email, request.AccountDetails.AccountNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user with account number: {AccountNumber}", request.AccountDetails.AccountNumber);
                return ErrorResponseModel.InternalServerError(UserErrorMessages.ErrorOccurredWhileRegistringUser, UserErrorMessages.ErrorOccurredWhileRegistringUserCode);
            }

            return ResponseModel.Ok(request, UserErrorMessages.UserRegistrationSuccess);
        }

        // Add these private helper methods to the class:
        private async Task<User> GetUserByEmail(string email)
        {
            try
            {
                return await _httpClientService.GetAsync<User>($"v1/perops/user/getuserbyemail/{email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User not found by email");
            }
            return null;
        }

        private async Task<User> GetUserByAccountNumber(string accountNumber)
        {
            try
            {
                return await _httpClientService.GetAsync<User>($"v1/perops/user/getuserbyaccountnumber/{accountNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User not found by account number");
            }
            return null;
        }

        private async Task<ResponseModel> AddUserData(User user)
        {
            try
            {
                return await _httpClientService.PostAsync<User, ResponseModel>("v1/perops/user/adduser", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding User:");
            }
            return null;
        }

        private async Task<User> GetUserData(long userId)
        {
            try
            {
                return await _httpClientService.GetAsync<User>("v1/perops/user/getuser/" + userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User not found");
            }
            return null;
        }

        /// <summary>
        /// List userpage service
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="searchString"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public async Task<ResponseModel> ListUserAsync(int pageSize, int pageNumber, string searchString, string sortBy)
        {
            try
            {
                _logger.LogInformation("Getting user page detail for page number: " + pageNumber);

                // Step 2: Check for existing user.
                List<User> users = await GetUserByPageAsync(pageSize, pageNumber, searchString, sortBy);
                _logger.LogInformation("User list retrieved successfully");

                return ResponseModel.Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Got list user error.");
                return ErrorResponseModel.InternalServerError(UserErrorMessages.ErrorOccurredWhileRegistringUser, UserErrorMessages.ErrorOccurredWhileRegistringUserCode);
            }
        }

        /// <summary>
        /// Load dashboard service
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel> LoadDashboardAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting user dashboard data for user id: " + userId);

                // Step 1: Get user list page.
                List<User> users = await GetUserByPageAsync(10, 1, "1=1", "1=1");
                _logger.LogInformation("User list retrieved successfully");

                Dictionary<string, dynamic> dashboardData = new Dictionary<string, dynamic>();
                dashboardData.Add("user-list", users);

                return ResponseModel.Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Got list user error.");
                return ErrorResponseModel.InternalServerError(UserErrorMessages.ErrorOccurredWhileRegistringUser, UserErrorMessages.ErrorOccurredWhileRegistringUserCode);
            }
        }

        private async Task<List<User>> GetUserByPageAsync(int pageSize, int pageNumber, string searchString, string sortBy)
        {
            try
            {
                var result = await _httpClientService.GetAsync<List<User>>($"v1/perops/user/getuserbypage/{pageSize}/{pageNumber}/{searchString}/{sortBy}");
                if (result == null)
                {
                    throw new Exception("Fail to get user list.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User not found");
                throw new Exception("Fail to get user list.");
            }
        }
    }
}
