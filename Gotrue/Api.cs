using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Supabase.Core;
using Supabase.Core.Attributes;
using Supabase.Core.Extensions;
using Supabase.Core.Interfaces;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using Supabase.Gotrue.Responses;
using static Supabase.Gotrue.Client;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
	public class Api : IGotrueApi<User, Session>
	{
		protected string Url { get; private set; }

		/// <summary>
		/// Function that can be set to return dynamic headers.
		/// 
		/// Headers specified in the constructor will ALWAYS take precendece over headers returned by this function.
		/// </summary>
		public Func<Dictionary<string, string>>? GetHeaders { get; set; }

		private Dictionary<string, string> _headers;
		protected Dictionary<string, string> Headers
		{
			get
			{
				return GetHeaders != null ? GetHeaders().MergeLeft(_headers) : _headers;
			}
			set
			{
				_headers = value;

				if (!_headers.ContainsKey("X-Client-Info"))
					_headers.Add("X-Client-Info", Util.GetAssemblyVersion(typeof(Client)));
			}
		}

		/// <summary>
		/// Creates a new user using their email address.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		public Api(string url, Dictionary<string, string>? headers = null)
		{
			Url = url;

			headers ??= new Dictionary<string, string>();
			_headers = headers;
		}

		/// <summary>
		/// Signs a user up using an email address and password.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="options">Optional Signup data.</param>
		/// <returns></returns>
		public async Task<Session?> SignUpWithEmail(string email, string password, SignUpOptions? options = null)
		{
			var body = new Dictionary<string, object> { { "email", email }, { "password", password } };

			string endpoint = $"{Url}/signup";

			if (options != null)
			{
				if (!string.IsNullOrEmpty(options.RedirectTo))
				{
					endpoint = Helpers.AddQueryParams(endpoint, new Dictionary<string, string> { { "redirect_to", options.RedirectTo! } }).ToString();
				}

				if (options.Data != null)
				{
					body.Add("data", options.Data);
				}
			}

			var response = await Helpers.MakeRequest(HttpMethod.Post, endpoint, body, Headers);

			if (!string.IsNullOrEmpty(response.Content))
			{
				// Gotrue returns a Session object for an auto-/pre-confirmed account
				var session = JsonConvert.DeserializeObject<Session>(response.Content!);

				// If account is unconfirmed, Gotrue returned the user object, so fill User data
				// in from the parsed response.
				if (session != null && session.User == null)
				{
					// Gotrue returns a User object for an unconfirmed account
					session.User = JsonConvert.DeserializeObject<User>(response.Content!);
				}

				return session;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Logs in an existing user using their email address.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Task<Session?> SignInWithEmail(string email, string password)
		{
			var body = new Dictionary<string, object> { { "email", email }, { "password", password } };
			return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=password", body, Headers);
		}


		/// <summary>
		/// Allows signing in with an ID token issued by certain supported providers.
		/// The [idToken] is verified for validity and a new session is established.
		/// This method of signing in only supports [Provider.Google] or [Provider.Apple].
		/// </summary>
		/// <param name="provider">A supported provider (Google, Apple)</param>
		/// <param name="idToken"></param>
		/// <param name="nonce"></param>
		/// <param name="captchaToken"></param>
		/// <returns></returns>
		/// <exception cref="InvalidProviderException"></exception>
		public Task<Session?> SignInWithIdToken(Provider provider, string idToken, string? nonce = null, string? captchaToken = null)
		{
			if (provider != Provider.Google && provider != Provider.Apple)
				throw new InvalidProviderException($"Provider must either be: `Provider.Google` or `Provider.Apple`.");

			var body = new Dictionary<string, object?>
			{
				{"provider", Core.Helpers.GetMappedToAttr(provider).Mapping },
				{"id_token", idToken },
			};

			if (!string.IsNullOrEmpty(nonce))
				body.Add("nonce", nonce);

			if (!string.IsNullOrEmpty(captchaToken))
				body.Add("gotrue_meta_security", new Dictionary<string, object?> { { "captcha_token", captchaToken } });


			return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=id_token", body, Headers);
		}

		/// <summary>
		/// Sends a magic login link to an email address.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public Task<BaseResponse> SendMagicLinkEmail(string email, SignInOptions? options = null)
		{
			var data = new Dictionary<string, string> { { "email", email } };

			string endpoint = $"{Url}/magiclink";

			if (options != null)
			{
				if (!string.IsNullOrEmpty(options.RedirectTo))
				{
					endpoint = Helpers.AddQueryParams(endpoint, new Dictionary<string, string> { { "redirect_to", options.RedirectTo! } }).ToString();
				}
			}

			return Helpers.MakeRequest(HttpMethod.Post, endpoint, data, Headers);
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
		/// <param name="options">Optional Signup data.</param>
		/// <returns></returns>
		public Task<Session?> SignUpWithPhone(string phone, string password, SignUpOptions? options = null)
		{
			var body = new Dictionary<string, object> {
				{ "phone", phone },
				{ "password", password },
			};

			string endpoint = $"{Url}/signup";

			if (options != null)
			{
				if (!string.IsNullOrEmpty(options.RedirectTo))
				{
					endpoint = Helpers.AddQueryParams(endpoint, new Dictionary<string, string> { { "redirect_to", options.RedirectTo! } }).ToString();
				}

				if (options.Data != null)
				{
					body.Add("data", options.Data);
				}
			}

			return Helpers.MakeRequest<Session>(HttpMethod.Post, endpoint, body, Headers);
		}

		/// <summary>
		/// Logs in an existing user using their phone number and password.
		/// </summary>
		/// <param name="phone">The phone number of the user.</param>
		/// <param name="password">The password of the user.</param>
		/// <returns></returns>
		public Task<Session?> SignInWithPhone(string phone, string password)
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
		public Task<Session?> VerifyMobileOTP(string phone, string token, MobileOtpType type)
		{
			var data = new Dictionary<string, string> {
				{ "phone", phone },
				{ "token", token },
				{ "type", Core.Helpers.GetMappedToAttr(type).Mapping }
			};
			return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/verify", data, Headers);
		}

		/// <summary>
		/// Send User supplied Mobile OTP to be verified
		/// </summary>
		/// <param name="phone">The user's phone number WITH international prefix</param>
		/// <param name="token">token that user was sent to their mobile phone</param>
		/// <returns></returns>
		public Task<Session?> VerifyEmailOTP(string email, string token, EmailOtpType type)
		{
			var data = new Dictionary<string, string> {
				{ "email", email },
				{ "token", token },
				{ "type", Core.Helpers.GetMappedToAttr(type).Mapping }
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
		/// Generates the relevant login URI for a third-party provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public ProviderAuthState GetUriForProvider(Provider provider, SignInOptions? options = null)
		{
			var builder = new UriBuilder($"{Url}/authorize");
			var result = new ProviderAuthState(builder.Uri);

			var attr = Core.Helpers.GetMappedToAttr(provider);
			var query = HttpUtility.ParseQueryString("");
			options ??= new SignInOptions();

			if (options.FlowType == OAuthFlowType.PKCE)
			{
				var codeVerifier = Helpers.GenerateNonce();
				var codeChallenge = Helpers.GeneratePKCENonceVerifier(codeVerifier);

				query.Add("flow_type", "pkce");
				query.Add("code_challenge", codeChallenge);
				query.Add("code_challenge_method", "s256");

				result.PKCEVerifier = codeVerifier;
			}

			if (attr is MapToAttribute mappedAttr)
			{
				query.Add("provider", mappedAttr.Mapping);

				if (!string.IsNullOrEmpty(options.Scopes))
					query.Add("scopes", options.Scopes);

				if (!string.IsNullOrEmpty(options.RedirectTo))
					query.Add("redirect_to", options.RedirectTo);

				if (options.QueryParams != null)
					foreach (var param in options.QueryParams)
						query[param.Key] = param.Value;

				builder.Query = query.ToString();

				result.Uri = builder.Uri;
				return result;
			}

			throw new Exception("Unknown provider");
		}

		/// <summary>
		/// Log in an existing user via a third-party provider.
		/// </summary>
		/// <param name="codeVerifier">Generated verifier (probably from GetUrlForProvider)</param>
		/// <param name="authCode">The received Auth Code Callback</param>
		/// <returns></returns>
		public Task<Session?> ExchangeCodeForSession(string codeVerifier, string authCode)
		{
			var url = new UriBuilder($"{Url}/token?grant_type=pkce");
			var body = new Dictionary<string, object>
			{
				{ "auth_code", authCode },
				{ "code_verifier", codeVerifier }
			};

			return Helpers.MakeRequest<Session>(HttpMethod.Post, url.ToString(), body, Headers);
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
		public Task<User?> GetUser(string jwt)
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
		public Task<User?> GetUserById(string jwt, string userId)
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
		public Task<User?> UpdateUser(string jwt, UserAttributes attributes)
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
		public Task<UserList<User>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null, int? perPage = null)
		{
			var data = TransformListUsersParams(filter, sortBy, sortOrder, page, perPage);

			return Helpers.MakeRequest<UserList<User>>(HttpMethod.Get, $"{Url}/admin/users", data, CreateAuthedRequestHeaders(jwt));
		}

		internal Dictionary<string, string> TransformListUsersParams(string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null, int? perPage = null)
		{
			var query = new Dictionary<string, string> { };

			if (filter != null && !string.IsNullOrWhiteSpace(filter))
			{
				query.Add("filter", filter);
			}

			if (!string.IsNullOrWhiteSpace(sortBy))
			{
				var mapTo = Core.Helpers.GetMappedToAttr(sortOrder);
				query.Add("sort", $"{sortBy} {mapTo.Mapping}");
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
		public Task<User?> CreateUser(string jwt, AdminUserAttributes? attributes = null)
		{
			if (attributes == null)
			{
				attributes = new AdminUserAttributes();
			}

			return Helpers.MakeRequest<User>(HttpMethod.Post, $"{Url}/admin/users", attributes, CreateAuthedRequestHeaders(jwt));
		}

		/// <summary>
		/// Update user by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task<User?> UpdateUserById(string jwt, string userId, UserAttributes userData)
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
		public Task<Session?> RefreshAccessToken(string refreshToken)
		{
			var data = new Dictionary<string, string> {
				{ "refresh_token", refreshToken }
			};

			return Helpers.MakeRequest<Session>(HttpMethod.Post, $"{Url}/token?grant_type=refresh_token", data, Headers);
		}
	}
}
