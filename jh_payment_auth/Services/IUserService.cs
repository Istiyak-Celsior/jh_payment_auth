using jh_payment_auth.DTOs;
using jh_payment_auth.Models;
using Microsoft.AspNetCore.Mvc;

namespace jh_payment_auth.Services
{
    /// <summary>
    /// Defines contract for user-related operations, such as registration.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// User registration process, including validation, duplication checks, password hashing, and storing user data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>ResponseModel</returns>
        Task<ResponseModel> RegisterUserAsync(UserRegistrationRequest request);

        /// <summary>
        /// List user detail by page
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="searchString"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        Task<ResponseModel> ListUserAsync(int pageSize, int pageNumber, string searchString, string sortBy);

        /// <summary>
        /// Load dashboard service interface
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseModel> LoadDashboardAsync(int userId);
    }
}
