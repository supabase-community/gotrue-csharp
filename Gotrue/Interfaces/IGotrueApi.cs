using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Core.Interfaces;
using Supabase.Gotrue.CustomProviders;
using Supabase.Gotrue.Mfa;
using Supabase.Gotrue.OAuth;
using Supabase.Gotrue.OAuthAuthorization;
using Supabase.Gotrue.Responses;
using Supabase.Gotrue.Responses.CustomProviders;
using Supabase.Gotrue.Responses.OAuth;
using static Supabase.Gotrue.Constants;

#pragma warning disable CS1591

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueApi<TUser, TSession> : IGettableHeaders
        where TUser : User
        where TSession : Session
    {
        Task<TUser?> CreateUser(string jwt, AdminUserAttributes? attributes = null);
        Task<BaseResponse> DeleteUser(string uid, string jwt, bool softDelete = false);
        Task<TUser?> GetUser(string jwt);
        Task<TUser?> GetUserById(string jwt, string userId);
        Task<BaseResponse> InviteUserByEmail(
            string email,
            string jwt,
            InviteUserByEmailOptions? options = null
        );
        Task<UserList<TUser>?> ListUsers(
            string jwt,
            string? filter = null,
            string? sortBy = null,
            SortOrder sortOrder = SortOrder.Descending,
            int? page = null,
            int? perPage = null
        );
        Task<TSession?> RefreshAccessToken(string accessToken, string refreshToken);
        Task<BaseResponse> ResetPasswordForEmail(string email);
        Task<ResetPasswordForEmailState> ResetPasswordForEmail(
            ResetPasswordForEmailOptions options
        );
        Task<BaseResponse> SendMagicLinkEmail(string email, SignInOptions? options = null);
        Task<BaseResponse> SendMobileOTP(string phone);
        Task<TSession?> SignInWithIdToken(
            Provider provider,
            string idToken,
            string? accessToken = null,
            string? nonce = null,
            string? captchaToken = null
        );
        Task<TSession?> SignInWithEmail(string email, string password);
        Task<TSession?> SignInWithPhone(string phone, string password);
        Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessEmailOptions options);
        Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessPhoneOptions options);
        Task<TSession?> SignInAnonymously(SignInAnonymouslyOptions? options = null);
        Task<SSOResponse?> SignInWithSSO(Guid providerId, SignInWithSSOOptions? options = null);
        Task<SSOResponse?> SignInWithSSO(string domain, SignInWithSSOOptions? options = null);
        Task<BaseResponse> SignOut(string jwt, SignOutScope scope = SignOutScope.Global);
        Task<TSession?> SignUpWithEmail(
            string email,
            string password,
            SignUpOptions? options = null
        );
        Task<TSession?> SignUpWithPhone(
            string phone,
            string password,
            SignUpOptions? options = null
        );
        Task<TUser?> UpdateUser(string jwt, UserAttributes attributes);
        Task<TUser?> UpdateUserById(string jwt, string userId, UserAttributes userData);
        Task<TSession?> VerifyMobileOTP(string phone, string token, MobileOtpType type);
        Task<TSession?> VerifyEmailOTP(string email, string token, EmailOtpType type);
        Task<TSession?> VerifyTokenHash(string tokenHash, EmailOtpType type);
        Task<BaseResponse> Reauthenticate(string userJwt);
        Task<BaseResponse> Resend(ResendParams resendParams);
        ProviderAuthState GetUriForProvider(Provider provider, SignInOptions? options = null);
        Task<Session?> ExchangeCodeForSession(string codeVerifier, string authCode);
        Task<Settings?> Settings();
        Task<BaseResponse> GenerateLink(string jwt, GenerateLinkOptions options);
        Task<MfaEnrollResponse?> Enroll(string jwt, MfaEnrollParams mfaEnrollParams);
        Task<MfaChallengeResponse?> Challenge(string jwt, MfaChallengeParams mfaChallengeParams);
        Task<MfaVerifyResponse?> Verify(string jwt, MfaVerifyParams mfaVerifyParams);
        Task<MfaUnenrollResponse?> Unenroll(string jwt, MfaUnenrollParams mfaVerifyParams);
        Task<BaseResponse> ListFactors(string jwt, MfaAdminListFactorsParams listFactorsParams);
        Task<MfaAdminDeleteFactorResponse?> DeleteFactor(
            string jwt,
            MfaAdminDeleteFactorParams deleteFactorParams
        );
        Task<OAuthClientResponse> ListOAuthClients(string jwt);
        Task<OAuthClient> CreateOAuthClient(string jwt, CreateOAuthClient client);
        Task<OAuthClient> GetOAuthClient(string jwt, string clientId);
        Task<OAuthClient> UpdateOAuthClient(string jwt, string clientId, UpdateOAuthClient client);
        Task<BaseResponse> DeleteOAuthClient(string jwt, string clientId);
        Task<OAuthClient> RegenerateOAuthClientSecret(string jwt, string clientId);
        Task<CustomProviderResponse> ListCustomProviders(string jwt);
        Task<CustomProvider> CreateCustomProvider(string jwt, CreateCustomProvider provider);
        Task<CustomProvider> GetCustomProvider(string jwt, string providerId);
        Task<CustomProvider> UpdateCustomProvider(
            string jwt,
            string providerId,
            UpdateCustomProvider provider
        );
        Task<BaseResponse> DeleteCustomProvider(string jwt, string providerId);

        /// <summary>
        /// Links an oauth identity to an existing user.
        ///
        /// This method requires the PKCE flow.
        /// </summary>
        /// <param name="token">User's token</param>
        /// <param name="provider">Provider to Link</param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<ProviderAuthState> LinkIdentity(
            string token,
            Provider provider,
            SignInOptions options
        );

        /// <summary>
        /// Unlinks an identity from a user by deleting it. The user will no longer be able to sign in with that identity once it's unlinked.
        /// </summary>
        /// <param name="token">User's token</param>
        /// <param name="userIdentity">Identity to be unlinked</param>
        /// <returns></returns>
        Task<bool> UnlinkIdentity(string token, UserIdentity userIdentity);
        
	    #region OAuth

		/// <summary>
		/// Retrieves details about an OAuth authorization request.
		/// Used to display consent information to the user.
		/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
		///
		/// This method returns one of two response types:
		/// - OAuthAuthorizationDetails: User needs to consent - show consent page with client info
		/// - OAuthRedirect: User already consented - redirect immediately to the OAuth client
		///
		/// Use type checking to distinguish between the responses:
		/// Check if Detail is not null to show consent page, otherwise redirect to Redirect.RedirectUrl
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="authorizationId">The authorization ID from the authorization request</param>
		/// <returns>Authorization details or redirect URL depending on consent status</returns>
		public Task<OAuthAuthorizationDetail?> GetAuthorizationDetails(string jwt, string authorizationId);

		/// <summary>
		/// Approves an OAuth authorization request.
		/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
		/// 
		/// After approval, the user's consent is stored and an authorization code is generated.
		/// The response contains a complete redirect URL with the authorization code and state.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="authorizationId">The authorization ID to approve</param>
		/// <param name="options">Optional parameters. If skipBrowserRedirect is false (default), automatically redirects the browser to the OAuth client. If true, returns the redirect_url without automatic redirect (useful for custom handling).</param>
		/// <returns>Redirect URL to send the user back to the OAuth client with authorization code</returns>
		public Task<OAuthAuthorizationRedirect?> ApproveAuthorization(string jwt, string authorizationId);

		/// <summary>
		/// Denies an OAuth authorization request.
		/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
		/// 
		/// After denial, the response contains a redirect URL with an OAuth error
		/// (access_denied) to inform the OAuth client that the user rejected the request.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="authorizationId">The authorization ID to deny</param>
		/// <param name="options">Optional parameters. If skipBrowserRedirect is false (default), automatically redirects the browser to the OAuth client. If true, returns the redirect_url without automatic redirect (useful for custom handling).</param>
		/// <returns>Redirect URL to send the user back to the OAuth client with error information</returns>
		public Task<OAuthAuthorizationRedirect?> DenyAuthorization(string jwt, string authorizationId);

		/// <summary>
		/// Lists all OAuth grants that the authenticated user has authorized.
		/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <returns>Response with array of OAuth grants with client information and granted scopes</returns>
		public Task<List<OAuthAuthorizationGrant>?> ListGrants(string jwt);

		/// <summary>
		/// Revokes a user's OAuth grant for a specific client.
		/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
		/// 
		/// Revocation marks consent as revoked, deletes active sessions for that OAuth client,
		/// and invalidates associated refresh tokens.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="clientId">The OAuth grant identifier to revoke</param>
		/// <returns>Empty response on successful revocation</returns>
		public Task<BaseResponse> RevokeGrant(string jwt, string clientId);

		#endregion
    }
}
