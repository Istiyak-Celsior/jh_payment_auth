using jh_payment_auth.Entity;
using jh_payment_auth.Models;

namespace jh_payment_auth.Services
{
    public interface IAuthService
    {
        Task<ResponseModel> Login(LoginRequest request);
        Task<User> ValidateUser(LoginRequest request);
    }
}
