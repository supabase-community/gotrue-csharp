using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Supabase.Gotrue.Attributes;
using Supabase.Gotrue.Responses;
using static Supabase.Gotrue.Client;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
    public class Api
    {
        protected string Url { get; private set; }
        protected Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new user using their email address.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        public Api(string url, Dictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            if (!Headers.ContainsKey("X-Client-Info"))
            {
                Headers.Add("X-Client-Info", Util.GetAssemblyVersion());
            }
        }

        /// <summary>
        /// Signs a user up using an email address and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Session> SignUpWithEmail(string email, string password)
        {
            var data = new Dictionary<string, string> { { "email", email }, { "password", password } };
            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/signup", data, Headers);
        }

        /// <summary>
        /// Logs in an existing user using their email address.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Session> SignInWithEmail(string email, string password)
        {
            var data = new Dictionary<string, string> { { "email", email }, { "password", password } };
            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=password", data, Headers);
        }

        /// <summary>
        /// Sends a magic login link to an email address.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<BaseResponse> SendMagicLinkEmail(string email)
        {
            var data = new Dictionary<string, string> { { "email", email } };
            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/magiclink", data, Headers);
        }

        /// <summary>
        /// Sends an invite link to an email address.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
        /// <returns></returns>
        public Task<BaseResponse> InviteUserByEmail(string email, string jwt)
        {
            var data = new Dictionary<string, string> { { "email", email } };
            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/invite", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Signs up a new user using their phone number and a password.The phone number of the user.
        /// </summary>
        /// <param name="phone">The phone number of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="metadata">User Metadata (optional)</param>
        /// <returns></returns>
        public Task<Session> SignUpWithPhone(string phone, string password, Dictionary<string, string> metadata = null)
        {
            var data = new Dictionary<string, object> {
                { "phone", phone },
                { "password", password },
                { "data", metadata }
            };
            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/signup", data, Headers);
        }

        /// <summary>
        /// Logs in an existing user using their phone number and password.
        /// </summary>
        /// <param name="phone">The phone number of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns></returns>
        public Task<Session> SignInWithPhone(string phone, string password)
        {
            var data = new Dictionary<string, object> {
                { "phone", phone },
                { "password", password },
            };
            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=password", data, Headers);
        }

        /// <summary>
        /// Sends a mobile OTP via SMS. Will register the account if it doesn't already exist
        /// </summary>
        /// <param name="phone">phone The user's phone number WITH international prefix</param>
        /// <returns></returns>
        public Task<BaseResponse> SendMobileOTP(string phone)
        {
            var data = new Dictionary<string, string> { { "phone", phone } };
            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/otp", data, Headers);
        }

        /// <summary>
        /// Send User supplied Mobile OTP to be verified
        /// </summary>
        /// <param name="phone">The user's phone number WITH international prefix</param>
        /// <param name="token">token that user was sent to their mobile phone</param>
        /// <returns></returns>
        public Task<Session> VerifyMobileOTP(string phone, string token)
        {
            var data = new Dictionary<string, string> {
                { "phone", phone },
                { "token", token },
                { "type", "sms" }
            };
            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/verify", data, Headers);
        }

        /// <summary>
        /// Sends a reset request to an email address.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<BaseResponse> ResetPasswordForEmail(string email)
        {
            var data = new Dictionary<string, string> { { "email", email } };
            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/recover", data, Headers);
        }

        /// <summary>
        /// Create a temporary object with all configured headers and adds the Authorization token to be used on request methods
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        internal Dictionary<string, string> CreateAuthedRequestHeaders(string jwt)
        {
            var headers = new Dictionary<string, string>(Headers);

            headers["Authorization"] = $"Bearer {jwt}";

            return headers;
        }

        /// <summary>
        /// Generates the relevant login URL for a third-party provider.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
        /// <returns></returns>
        internal string GetUrlForProvider(Provider provider, string scopes = null)
        {
            var builder = new UriBuilder($"{Url}/authorize");
            var attr = provider.GetType().GetField(provider.ToString()).GetCustomAttributes(typeof(MapToAttribute), true).First();

            if (attr is MapToAttribute mappedAttr)
            {
                var query = HttpUtility.ParseQueryString("");
                query.Add("provider", mappedAttr.Mapping);
                query.Add("scopes", scopes);

                builder.Query = query.ToString();
                return builder.ToString();
            }

            throw new Exception("Unknown provider");
        }

        /// <summary>
        /// Removes a logged-in session.
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public Task<BaseResponse> SignOut(string jwt)
        {
            var data = new Dictionary<string, string> { };

            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/logout", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Gets User Details
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public Task<User> GetUser(string jwt)
        {
            var data = new Dictionary<string, string> { };

            return Helpers.MakeRequest<User>(HttpMethod.Get, $"{Url}/user", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Get User details by Id
        /// </summary>
        /// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<User> GetUserById(string jwt,string userId)
        {
            var data = new Dictionary<string, string> { };

            return Helpers.MakeRequest<User>(HttpMethod.Get, $"{Url}/admin/users/{userId}", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Updates the User data
        /// </summary>
        /// <param name="jwt"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public Task<User> UpdateUser(string jwt, UserAttributes attributes)
        {
            return Helpers.MakeRequest<User>(HttpMethod.Put, $"{Url}/user", attributes, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Lists users
        /// </summary>
        /// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
        /// <param name="filter">A string for example part of the email</param>
        /// <param name="sortBy">Snake case string of the given key, currently only created_at is suppported</param>
        /// <param name="sortOrder">asc or desc, if null desc is used</param>
        /// <param name="page">page to show for pagination</param>
        /// <param name="perPage">items per page for pagination</param>
        /// <returns></returns>
        public Task<UserList> ListUsers(string jwt, string filter = null, string sortBy = null, SORT_ORDER sortOrder = SORT_ORDER.DESC, int? page = null, int? perPage = null)
        {
            var data = TransformListUsersParams(filter, sortBy, sortOrder, page, perPage);

            return Helpers.MakeRequest<UserList>(HttpMethod.Get, $"{Url}/admin/users", data, CreateAuthedRequestHeaders(jwt));
        }

        internal Dictionary<string, string> TransformListUsersParams(string filter = null, string sortBy = null, SORT_ORDER sortOrder = SORT_ORDER.DESC, int? page = null, int? perPage = null)
        {
            var query = new Dictionary<string, string> { };

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query.Add("filter", filter);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query.Add("sort", $"{sortBy} {sortOrder.ToString().ToLower()}");
            }

            if (page.HasValue)
            {
                query.Add("page", page.Value.ToString());
            }

            if (perPage.HasValue)
            {
                query.Add("per_page", perPage.Value.ToString());
            }

            return query;
        }

        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public Task<User> CreateUser(string jwt, string email, string password, object userData = null)
        {
            var data = new Dictionary<string, object> { { "email", email }, { "password", password } };

            if(userData != null)
            {
                data.Add("data", userData);
            }

            return Helpers.MakeRequest<User>(HttpMethod.Post, $"{Url}/admin/users", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Update user by Id
        /// </summary>
        /// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<User> UpdateUserById(string jwt, string userId, UserAttributes userData)
        {
            return Helpers.MakeRequest<User>(HttpMethod.Put, $"{Url}/admin/users/{userId}", userData, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="uid">The user uid you want to remove.</param>
        /// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
        /// <returns></returns>
        public Task<BaseResponse> DeleteUser(string uid, string jwt)
        {
            var data = new Dictionary<string, string> { };
            return Helpers.MakeRequest(HttpMethod.Delete, $"{Url}/admin/users/{uid}", data, CreateAuthedRequestHeaders(jwt));
        }

        /// <summary>
        /// Generates a new JWT
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public Task<Session> RefreshAccessToken(string refreshToken)
        {
            var data = new Dictionary<string, string> {
                { "refresh_token", refreshToken }
            };

            return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=refresh_token", data, Headers);
        }
    }
}
