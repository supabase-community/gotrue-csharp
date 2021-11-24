using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Attributes;
using static Supabase.Gotrue.Client;

namespace Supabase.Gotrue
{
    /// <summary>
    /// A Stateless Gotrue Client
    /// </summary>
    /// <example>
    /// var options = new StatelessClientOptions { Url = "https://mygotrueurl.com" };
    /// var user = await client.SignIn("user@email.com", "fancyPassword", options);
    /// </example>
    public static class StatelessClient
    {

        public static Api GetApi(StatelessClientOptions options) => new Api(options.Url, options.Headers);

        /// <summary>
        /// Signs up a user by email address
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Task<Session> SignUp(string email, string password, StatelessClientOptions options) => SignUp(SignUpType.Email, email, password, options);

        /// <summary>
        /// Signs up a user
        /// </summary>
        /// <param name="type">Type of signup</param>
        /// <param name="identifier">Phone or Email</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<Session> SignUp(SignUpType type, string identifier, string password, StatelessClientOptions options)
        {
            try
            {
                var api = GetApi(options);
                Session session = null;
                switch (type)
                {
                    case SignUpType.Email:
                        session = await api.SignUpWithEmail(identifier, password);
                        break;
                    case SignUpType.Phone:
                        session = await api.SignUpWithPhone(identifier, password);
                        break;
                }

                if (session?.User?.ConfirmedAt != null)
                {
                    return session;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }


        /// <summary>
        /// Sends a Magic email login link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<bool> SignIn(string email, StatelessClientOptions options)
        {
            try
            {
                await GetApi(options).SendMagicLinkEmail(email);
                return true;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Sends a Magic email login link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static Task<bool> SendMagicLink(string email, StatelessClientOptions options) => SignIn(email, options);

        /// <summary>
        /// Signs in a User.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Task<Session> SignIn(string email, string password, StatelessClientOptions options) => SignIn(SignInType.Email, email, password, options);

        /// <summary>
        /// Log in an existing user, or login via a third-party provider.
        /// </summary>
        /// <param name="type">Type of Credentials being passed</param>
        /// <param name="identifierOrToken">An email, phone, or RefreshToken</param>
        /// <param name="password">Password to account (optional if `RefreshToken`)</param>
        /// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
        /// <returns></returns>
        public static async Task<Session> SignIn(SignInType type, string identifierOrToken, string password = null, StatelessClientOptions options = null)
        {
            try
            {
                var api = GetApi(options);
                Session session = null;
                switch (type)
                {
                    case SignInType.Email:
                        session = await api.SignInWithEmail(identifierOrToken, password);
                        break;
                    case SignInType.Phone:
                        if (string.IsNullOrEmpty(password))
                        {
                            var response = await api.SendMobileOTP(identifierOrToken);
                            return null;
                        }
                        else
                        {
                            session = await api.SignInWithPhone(identifierOrToken, password);
                        }
                        break;
                    case SignInType.RefreshToken:
                        session = await RefreshToken(identifierOrToken, options);
                        break;
                }

                if (session?.User?.ConfirmedAt != null)
                {
                    return session;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Retrieves a Url to redirect to for signing in with a <see cref="Provider"/>.
        ///
        /// This method will need to be combined with <see cref="GetSessionFromUrl(Uri, bool)"/> when the
        /// Application receives the Oauth Callback.
        /// </summary>
        /// <example>
        /// var client = Supabase.Gotrue.Client.Initialize(options);
        /// var url = client.SignIn(Provider.Github);
        ///
        /// // Do Redirect User
        ///
        /// // Example code
        /// Application.HasRecievedOauth += async (uri) => {
        ///     var session = await client.GetSessionFromUri(uri, true);
        /// }
        /// </example>
        /// <param name="provider"></param>
        /// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
        /// <returns></returns>
        public static string SignIn(Provider provider, StatelessClientOptions options, string scopes = null) => GetApi(options).GetUrlForProvider(provider, scopes);

        /// <summary>
        /// Log in a user given a User supplied OTP received via mobile.
        /// </summary>
        /// <param name="phone">The user's phone number.</param>
        /// <param name="token">Token sent to the user's phone.</param>
        /// <returns></returns>
        public static async Task<Session> VerifyOTP(string phone, string token, StatelessClientOptions options)
        {
            try
            {
                var session = await GetApi(options).VerifyMobileOTP(phone, token);

                if (session?.AccessToken != null)
                {
                    return session;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Updates a User.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static async Task<User> Update(string accessToken, UserAttributes attributes, StatelessClientOptions options)
        {
            try
            {
                var result = await GetApi(options).UpdateUser(accessToken, attributes);
                return result;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Sends an invite email link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
        /// <returns></returns>
        public static async Task<bool> InviteUserByEmail(string email, string jwt, StatelessClientOptions options)
        {
            try
            {
                var response = await GetApi(options).InviteUserByEmail(email, jwt);
                response.ResponseMessage.EnsureSuccessStatusCode();
                return true;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Deletes a User.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
        /// <returns></returns>
        public static async Task<bool> DeleteUser(string uid, string jwt, StatelessClientOptions options)
        {
            try
            {
                var result = await GetApi(options).DeleteUser(uid, jwt);
                result.ResponseMessage.EnsureSuccessStatusCode();
                return true;
            }
            catch (RequestException ex)
            {
                throw ExceptionHandler.Parse(ex);
            }
        }

        /// <summary>
        /// Parses a <see cref="Session"/> out of a <see cref="Uri"/>'s Query parameters.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="storeSession"></param>
        /// <returns></returns>
        public static async Task<Session> GetSessionFromUrl(Uri uri, StatelessClientOptions options)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);

            var errorDescription = query.Get("error_description");

            if (!string.IsNullOrEmpty(errorDescription))
                throw new Exception(errorDescription);

            var accessToken = query.Get("access_token");

            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("No access_token detected.");

            var expiresIn = query.Get("expires_in");

            if (string.IsNullOrEmpty(expiresIn))
                throw new Exception("No expires_in detected.");

            var refreshToken = query.Get("refresh_token");

            if (string.IsNullOrEmpty(refreshToken))
                throw new Exception("No refresh_token detected.");

            var tokenType = query.Get("token_type");

            if (string.IsNullOrEmpty(tokenType))
                throw new Exception("No token_type detected.");

            var user = await GetApi(options).GetUser(accessToken);

            var session = new Session
            {
                AccessToken = accessToken,
                ExpiresIn = int.Parse(expiresIn),
                RefreshToken = refreshToken,
                TokenType = tokenType,
                User = user
            };

            return session;
        }

        /// <summary>
        /// Refreshes a Token
        /// </summary>
        /// <returns></returns>
        public static async Task<Session> RefreshToken(string refreshToken, StatelessClientOptions options) => await GetApi(options).RefreshAccessToken(refreshToken);

        /// <summary>
        /// Class represention options available to the <see cref="Client"/>.
        /// </summary>
        public class StatelessClientOptions
        {
            /// <summary>
            /// Gotrue Endpoint
            /// </summary>
            public string Url { get; set; } = Constants.GOTRUE_URL;

            /// <summary>
            /// Headers to be sent with subsequent requests.
            /// </summary>
            public Dictionary<string, string> Headers = new Dictionary<string, string>(Constants.DEFAULT_HEADERS);
        }
    }

}