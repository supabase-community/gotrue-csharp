using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Supabase.Gotrue.Attributes;
using Supabase.Gotrue.Responses;
using static Supabase.Gotrue.Client;

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
        /// <returns></returns>
        public Task<BaseResponse> InviteUserByEmail(string email)
        {
            var data = new Dictionary<string, string> { { "email", email } };
            return Helpers.MakeRequest(HttpMethod.Post, $"{Url}/invite", data, Headers);
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
        /// <returns></returns>
        internal string GetUrlForProvider(Provider provider)
        {
            var attr = provider.GetType().GetField(provider.ToString()).GetCustomAttributes(typeof(MapToAttribute), true).First();
            if (attr is MapToAttribute mappedAttr)
            {
                return $"{Url}/authorize?provider={mappedAttr.Mapping}";
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
