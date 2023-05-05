using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
	/// <summary>
	/// A Stateless Gotrue Client
	/// </summary>
	/// <example>
	/// var options = new StatelessClientOptions { Url = "https://mygotrueurl.com" };
	/// var user = await client.SignIn("user@email.com", "fancyPassword", options);
	/// </example>
	public class StatelessClient : IGotrueStatelessClient<User, Session>
	{

		public IGotrueApi<User, Session> GetApi(StatelessClientOptions options) => new Api(options.Url, options.Headers);

		/// <summary>
		/// Signs up a user by email address
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="options"></param>
		/// <param name="signUpOptions">Object containing redirectTo and optional user metadata (data)</param>
		/// <returns></returns>
		public Task<Session?> SignUp(string email, string password, StatelessClientOptions options, SignUpOptions? signUpOptions = null) => SignUp(SignUpType.Email, email, password, options, signUpOptions);

		/// <summary>
		/// Signs up a user
		/// </summary>
		/// <param name="type">Type of signup</param>
		/// <param name="identifier">Phone or Email</param>
		/// <param name="password"></param>
		/// <param name="options"></param>
		/// <param name="signUpOptions">Object containing redirectTo and optional user metadata (data)</param>
		/// <returns></returns>
		public async Task<Session?> SignUp(SignUpType type, string identifier, string password, StatelessClientOptions options, SignUpOptions? signUpOptions = null)
		{
			var api = GetApi(options);
			var session = type switch
			{
				SignUpType.Email => await api.SignUpWithEmail(identifier, password, signUpOptions),
				SignUpType.Phone => await api.SignUpWithPhone(identifier, password, signUpOptions),
				_ => null
			};

			if (session?.User?.ConfirmedAt != null || session?.User != null && options.AllowUnconfirmedUserSessions)
			{
				return session;
			}

			return null;
		}


		/// <summary>
		/// Sends a Magic email login link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <param name="signInOptions"></param>
		/// <returns></returns>
		public async Task<bool> SignIn(string email, StatelessClientOptions options, SignInOptions? signInOptions = null)
		{
			await GetApi(options).SendMagicLinkEmail(email, signInOptions);
			return true;
		}

		/// <summary>
		/// Sends a Magic email login link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <param name="signInOptions"></param>
		/// <returns></returns>
		public Task<bool> SendMagicLink(string email, StatelessClientOptions options, SignInOptions? signInOptions = null) => SignIn(email, options, signInOptions);

		/// <summary>
		/// Signs in a User.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public Task<Session?> SignIn(string email, string password, StatelessClientOptions options) => SignIn(SignInType.Email, email, password, options);

		/// <summary>
		/// Log in an existing user, or login via a third-party provider.
		/// </summary>
		/// <param name="type">Type of Credentials being passed</param>
		/// <param name="identifierOrToken">An email, phone, or RefreshToken</param>
		/// <param name="password">Password to account (optional if `RefreshToken`)</param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<Session?> SignIn(SignInType type, string identifierOrToken, string? password = null, StatelessClientOptions? options = null)
		{

			options ??= new StatelessClientOptions();

			var api = GetApi(options);
			Session? session;
			switch (type)
			{
				case SignInType.Email:
					session = await api.SignInWithEmail(identifierOrToken, password!);
					break;
				case SignInType.Phone:
					if (string.IsNullOrEmpty(password))
					{
						await api.SendMobileOTP(identifierOrToken);
						return null;
					}

					session = await api.SignInWithPhone(identifierOrToken, password!);
					break;
				case SignInType.RefreshToken:
					session = await RefreshToken(identifierOrToken, options);
					break;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			if (session?.User?.ConfirmedAt != null || session?.User != null && options.AllowUnconfirmedUserSessions)
			{
				return session;
			}

			return null;
		}

		/// <summary>
		/// Retrieves a Url to redirect to for signing in with a <see cref="Provider"/>.
		/// 
		/// This method will need to be combined with <see cref="GetSessionFromUrl(Uri,StatelessClientOptions)"/> when the
		/// Application receives the Oauth Callback.
		/// </summary>
		/// <example>
		/// var client = Supabase.Gotrue.Client.Initialize(options);
		/// var url = client.SignIn(Provider.Github);
		/// 
		/// // Do Redirect User
		/// 
		/// // Example code
		/// Application.HasReceivedOauth += async (uri) => {
		///     var session = await client.GetSessionFromUri(uri, true);
		/// }
		/// </example>
		/// <param name="provider"></param>
		/// <param name="options"></param>
		/// <param name="signInOptions"></param>
		/// <returns></returns>
		public ProviderAuthState SignIn(Provider provider, StatelessClientOptions options, SignInOptions? signInOptions = null) => GetApi(options).GetUriForProvider(provider, signInOptions);

		/// <summary>
		/// Logout a User
		/// This will revoke all refresh tokens for the user.
		/// JWT tokens will still be valid for stateless auth until they expire.
		/// </summary>
		/// <param name="jwt"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> SignOut(string jwt, StatelessClientOptions options)
		{
			var result = await GetApi(options).SignOut(jwt);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Log in a user given a User supplied OTP received via mobile.
		/// </summary>
		/// <param name="phone">The user's phone number.</param>
		/// <param name="token">Token sent to the user's phone.</param>
		/// <param name="options"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public async Task<Session?> VerifyOTP(string phone, string token, StatelessClientOptions options, MobileOtpType type = MobileOtpType.SMS)
		{
			var session = await GetApi(options).VerifyMobileOTP(phone, token, type);

			if (session?.AccessToken != null)
			{
				return session;
			}

			return null;
		}

		/// <summary>
		/// Log in a user give a user supplied OTP received via email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="token"></param>
		/// <param name="options"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public async Task<Session?> VerifyOTP(string email, string token, StatelessClientOptions options, EmailOtpType type = EmailOtpType.MagicLink)
		{
			var session = await GetApi(options).VerifyEmailOTP(email, token, type);

			if (session?.AccessToken != null)
			{
				return session;
			}

			return null;
		}


		/// <summary>
		/// Updates a User.
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="attributes"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<User?> Update(string accessToken, UserAttributes attributes, StatelessClientOptions options)
		{
			var result = await GetApi(options).UpdateUser(accessToken, attributes);
			return result;
		}

		/// <summary>
		/// Sends an invite email link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> InviteUserByEmail(string email, string jwt, StatelessClientOptions options)
		{
			var response = await GetApi(options).InviteUserByEmail(email, jwt);
			response.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Sends a reset request to an email address.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public async Task<bool> ResetPasswordForEmail(string email, StatelessClientOptions options)
		{
			var result = await GetApi(options).ResetPasswordForEmail(email);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Lists users
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="options"></param>
		/// <param name="filter">A string for example part of the email</param>
		/// <param name="sortBy">Snake case string of the given key, currently only created_at is supported</param>
		/// <param name="sortOrder">asc or desc, if null desc is used</param>
		/// <param name="page">page to show for pagination</param>
		/// <param name="perPage">items per page for pagination</param>
		/// <returns></returns>
		public async Task<UserList<User>?> ListUsers(string jwt, StatelessClientOptions options, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending,
			int? page = null, int? perPage = null)
		{
			return await GetApi(options).ListUsers(jwt, filter, sortBy, sortOrder, page, perPage);
		}

		/// <summary>
		/// Get User details by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="options"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<User?> GetUserById(string jwt, StatelessClientOptions options, string userId)
		{
			return await GetApi(options).GetUserById(jwt, userId);
		}

		/// <summary>
		/// Get User details by JWT. Can be used to validate a JWT.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a JWT that originates from a user.</param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<User?> GetUser(string jwt, StatelessClientOptions options)
		{
			return await GetApi(options).GetUser(jwt);
		}

		/// <summary>
		/// Create a user
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="options"></param>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public Task<User?> CreateUser(string jwt, StatelessClientOptions options, string email, string password, AdminUserAttributes? attributes = null)
		{
			attributes ??= new AdminUserAttributes();
			attributes.Email = email;
			attributes.Password = password;

			return CreateUser(jwt, options, attributes);
		}

		/// <summary>
		/// Create a user
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="options"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public async Task<User?> CreateUser(string jwt, StatelessClientOptions options, AdminUserAttributes attributes)
		{
			return await GetApi(options).CreateUser(jwt, attributes);
		}

		/// <summary>
		/// Update user by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="options"></param>
		/// <param name="userId"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public async Task<User?> UpdateUserById(string jwt, StatelessClientOptions options, string userId, AdminUserAttributes userData)
		{
			return await GetApi(options).UpdateUserById(jwt, userId, userData);
		}

		/// <summary>
		/// Deletes a User.
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> DeleteUser(string uid, string jwt, StatelessClientOptions options)
		{
			var result = await GetApi(options).DeleteUser(uid, jwt);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Parses a <see cref="Session"/> out of a <see cref="Uri"/>'s Query parameters.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<Session?> GetSessionFromUrl(Uri uri, StatelessClientOptions options)
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
		public async Task<Session?> RefreshToken(string refreshToken, StatelessClientOptions options) => await GetApi(options).RefreshAccessToken(refreshToken);


		/// <summary>
		/// Class representation options available to the <see cref="Client"/>.
		/// </summary>
		public class StatelessClientOptions
		{
			/// <summary>
			/// Gotrue Endpoint
			/// </summary>
			public string Url { get; set; } = GOTRUE_URL;

			/// <summary>
			/// Headers to be sent with subsequent requests.
			/// </summary>
			public readonly Dictionary<string, string> Headers = new Dictionary<string, string>();

			/// <summary>
			/// Very unlikely this flag needs to be changed except in very specific contexts.
			/// 
			/// Enables tests to be E2E tests to be run without requiring users to have
			/// confirmed emails - mirrors the Gotrue server's configuration.
			/// </summary>
			public bool AllowUnconfirmedUserSessions { get; set; }
		}
	}

}
