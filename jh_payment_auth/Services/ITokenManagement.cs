using jh_payment_auth.Models;

namespace jh_payment_auth.Services
{
    /// <summary>
    /// This interface defines the contract for token management services, including JWT token generation.
    /// </summary>
    public interface ITokenManagement
    {
        /// <summary>
        /// This method generates a JWT token for the specified user.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        string GenerateJwtToken(string userName);

        /// <summary>
        /// Creates a new refresh token for the specified user.
        /// </summary>
        /// <param name="userName">The name of the user for whom the refresh token is being created. Cannot be null or empty.</param>
        /// <returns>A <see cref="RefreshTokenModel"/> representing the newly created refresh token.</returns>
        RefreshTokenModel CreateRefreshToken(string userName);

        /// <summary>
        /// Refreshes the access token using the provided refresh token model.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to refresh the access token. Ensure
        /// that the provided  <paramref name="refreshTokenModel"/> contains valid and up-to-date information. The
        /// caller is responsible  for handling any exceptions that may occur during the refresh process.</remarks>
        /// <param name="refreshTokenModel">The model containing the refresh token and any additional information required to obtain a new access token.</param>
        /// <returns>A <see cref="TokenRefreshResult"/> containing the new access token, its expiration time, and any other
        /// relevant details.</returns>
        RefreshTokenResult RefreshAccessToken(RefreshTokenModel refreshTokenModel);
    }
}
