using System.Threading.Tasks;
using Supabase.Core.Interfaces;
using Supabase.Gotrue.CustomProviders;
using Supabase.Gotrue.Mfa;
using Supabase.Gotrue.OAuth;
using Supabase.Gotrue.Responses;
using Supabase.Gotrue.Responses.CustomProviders;
using Supabase.Gotrue.Responses.OAuth;

namespace Supabase.Gotrue.Interfaces
{
    /// <summary>
    /// Interface for the Gotrue Admin Client (auth).
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public interface IGotrueAdminClient<TUser> : IGettableHeaders
        where TUser : User
    {
        /// <summary>
        /// Creates a user using the admin key (not the anonymous key).
        /// Used in trusted server environments, not client apps.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        Task<TUser?> CreateUser(AdminUserAttributes attributes);

        /// <summary>
        /// Creates a user using the admin key (not the anonymous key).
        /// Used in trusted server environments, not client apps.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        Task<TUser?> CreateUser(
            string email,
            string password,
            AdminUserAttributes? attributes = null
        );

        /// <summary>
        /// Creates a user using the admin key (not the anonymous key).
        /// Used in trusted server environments, not client apps.
        /// </summary>
        Task<bool> DeleteUser(string uid, bool softDelete = false);

        /// <summary>
        /// Gets a user from a user's JWT. This is using the GoTrue server to validate a user's JWT.
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        Task<TUser?> GetUser(string jwt);

        /// <summary>
        /// Gets a user by ID from the server using the admin key (not the anonymous key).
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<TUser?> GetUserById(string userId);

        /// <summary>
        /// Sends an email to the user.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<bool> InviteUserByEmail(string email, InviteUserByEmailOptions? options = null);

        /// <summary>
        /// Lists users
        /// </summary>
        /// <param name="filter">A string for example part of the email</param>
        /// <param name="sortBy">Snake case string of the given key, currently only created_at is supported</param>
        /// <param name="sortOrder">asc or desc, if null desc is used</param>
        /// <param name="page">page to show for pagination</param>
        /// <param name="perPage">items per page for pagination</param>
        /// <returns></returns>
        Task<UserList<TUser>?> ListUsers(
            string? filter = null,
            string? sortBy = null,
            Constants.SortOrder sortOrder = Constants.SortOrder.Descending,
            int? page = null,
            int? perPage = null
        );

        /// <summary>
        /// Updates a User using the service key
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public Task<User?> Update(UserAttributes attributes);

        /// <summary>
        /// Update user by Id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public Task<User?> UpdateUserById(string userId, AdminUserAttributes userData);

        /// <summary>
        /// Generates email links and OTPs to be sent via a custom email provider.
        /// </summary>
        /// <param name="options">Options for this call. `Password` is required for <see cref="GenerateLinkOptions.LinkType.SignUp"/>, `Data` is an optional parameter for <see cref="GenerateLinkOptions.LinkType.SignUp"/>.</param>
        /// <returns></returns>
        public Task<GenerateLinkResponse?> GenerateLink(GenerateLinkOptions options);

        /// <summary>
        /// Lists all factors associated to a specific user.
        /// </summary>
        /// <param name="listFactorParams">A <see cref="MfaAdminListFactorsParams"/> object that contains the user id.</param>
        /// <returns>A list of <see cref="Factor"/> that this user has enabled.</returns>
        public Task<MfaAdminListFactorsResponse?> ListFactors(
            MfaAdminListFactorsParams listFactorsParams
        );

        /// <summary>
        /// Deletes a factor on a user. This will log the user out of all active sessions if the deleted factor was verified.
        /// </summary>
        /// <param name="listFactorParams">A <see cref="MfaAdminListFactorsParams"/> object that contains the user id.</param>
        /// <returns>A <see cref="MfaAdminDeleteFactorResponse"/> containing the deleted factor id.</returns>
        public Task<MfaAdminDeleteFactorResponse?> DeleteFactor(
            MfaAdminDeleteFactorParams deleteFactorParams
        );

        // Admin OAuth Client Management
        /// <summary>Lists all OAuth clients.</summary>
        Task<OAuthClientResponse> ListOAuthClients();

        /// <summary>Creates a new OAuth client.</summary>
        Task<OAuthClient> CreateOAuthClient(CreateOAuthClient client);

        /// <summary>Gets an OAuth client by ID.</summary>
        Task<OAuthClient> GetOAuthClient(string clientId);

        /// <summary>Updates an OAuth client.</summary>
        Task<OAuthClient> UpdateOAuthClient(string clientId, UpdateOAuthClient client);

        /// <summary>Deletes an OAuth client.</summary>
        Task<bool> DeleteOAuthClient(string clientId);

        /// <summary>Regenerates the client secret for an OAuth client.</summary>
        Task<OAuthClient> RegenerateOAuthClientSecret(string clientId);

        // Admin Custom Provider Management
        /// <summary>Lists all custom providers.</summary>
        Task<CustomProviderResponse> ListCustomProviders();

        /// <summary>Creates a new custom provider.</summary>
        Task<CustomProvider> CreateCustomProvider(CreateCustomProvider provider);

        /// <summary>Gets a custom provider by ID.</summary>
        Task<CustomProvider> GetCustomProvider(string providerId);

        /// <summary>Updates a custom provider.</summary>
        Task<CustomProvider> UpdateCustomProvider(
            string providerId,
            UpdateCustomProvider provider
        );

        /// <summary>Deletes a custom provider.</summary>
        Task<bool> DeleteCustomProvider(string providerId);
    }
}
