using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
	/// <inheritdoc />
	public class StatelessClient : IGotrueStatelessClient<User, Session>
	{
		/// <inheritdoc />
		public async Task<Settings?> Settings(StatelessClientOptions options)
		{
			var api = GetApi(options);
			return await api.Settings();
		}

		/// <inheritdoc />
		public IGotrueApi<User, Session> GetApi(StatelessClientOptions options) => new Api(options.Url, options.Headers);
		
		/// <inheritdoc />
		public Task<Session?> SignUp(string email, string password, StatelessClientOptions options, SignUpOptions? signUpOptions = null) => SignUp(SignUpType.Email, email, password, options, signUpOptions);
		
		/// <inheritdoc />
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
		
		/// <inheritdoc />
		public async Task<bool> SignIn(string email, StatelessClientOptions options, SignInOptions? signInOptions = null)
		{
			await GetApi(options).SendMagicLinkEmail(email, signInOptions);
			return true;
		}
		
		/// <inheritdoc />
		public Task<bool> SendMagicLink(string email, StatelessClientOptions options, SignInOptions? signInOptions = null) => SignIn(email, options, signInOptions);
		
		/// <inheritdoc />
		public Task<Session?> SignIn(string email, string password, StatelessClientOptions options) => SignIn(SignInType.Email, email, password, options);
		
		/// <inheritdoc />
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
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			if (session?.User?.ConfirmedAt != null || session?.User != null && options.AllowUnconfirmedUserSessions)
				return session;

			return null;
		}

		/// <inheritdoc />
		public ProviderAuthState SignIn(Provider provider, StatelessClientOptions options, SignInOptions? signInOptions = null) => GetApi(options).GetUriForProvider(provider, signInOptions);

		/// <inheritdoc />
		public async Task<bool> SignOut(string accessToken, StatelessClientOptions options)
		{
			var result = await GetApi(options).SignOut(accessToken);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}
		
		/// <inheritdoc />
		public async Task<Session?> VerifyOTP(string phone, string otpToken, StatelessClientOptions options, MobileOtpType type = MobileOtpType.SMS)
		{
			var session = await GetApi(options).VerifyMobileOTP(phone, otpToken, type);

			if (session?.AccessToken != null)
			{
				return session;
			}

			return null;
		}

		/// <inheritdoc />
		public async Task<Session?> VerifyOTP(string email, string otpToken, StatelessClientOptions options, EmailOtpType type = EmailOtpType.MagicLink)
		{
			var session = await GetApi(options).VerifyEmailOTP(email, otpToken, type);

			if (session?.AccessToken != null)
			{
				return session;
			}

			return null;
		}

		/// <inheritdoc />
		public async Task<User?> Update(string accessToken, UserAttributes attributes, StatelessClientOptions options)
		{
			var result = await GetApi(options).UpdateUser(accessToken, attributes);
			return result;
		}

		/// <inheritdoc />
		public async Task<bool> InviteUserByEmail(string email, string serviceRoleToken, StatelessClientOptions options, InviteUserByEmailOptions? invitationOptions = null)
		{
			var response = await GetApi(options).InviteUserByEmail(email, serviceRoleToken, invitationOptions);
			response.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <inheritdoc />
		public async Task<bool> ResetPasswordForEmail(string email, StatelessClientOptions options)
		{
			var result = await GetApi(options).ResetPasswordForEmail(email);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <inheritdoc />
		public async Task<UserList<User>?> ListUsers(string serviceRoleToken, StatelessClientOptions options, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending,
			int? page = null, int? perPage = null)
		{
			return await GetApi(options).ListUsers(serviceRoleToken, filter, sortBy, sortOrder, page, perPage);
		}

		/// <inheritdoc />
		public async Task<User?> GetUserById(string serviceRoleToken, StatelessClientOptions options, string userId)
		{
			return await GetApi(options).GetUserById(serviceRoleToken, userId);
		}

		/// <inheritdoc />
		public async Task<User?> GetUser(string serviceRoleToken, StatelessClientOptions options)
		{
			return await GetApi(options).GetUser(serviceRoleToken);
		}

		/// <inheritdoc />
		public Task<User?> CreateUser(string serviceRoleToken, StatelessClientOptions options, string email, string password, AdminUserAttributes? attributes = null)
		{
			attributes ??= new AdminUserAttributes();
			attributes.Email = email;
			attributes.Password = password;

			return CreateUser(serviceRoleToken, options, attributes);
		}

		/// <inheritdoc />
		public async Task<User?> CreateUser(string serviceRoleToken, StatelessClientOptions options, AdminUserAttributes attributes)
		{
			return await GetApi(options).CreateUser(serviceRoleToken, attributes);
		}

		/// <inheritdoc />
		public async Task<User?> UpdateUserById(string serviceRoleToken, StatelessClientOptions options, string userId, AdminUserAttributes userData)
		{
			return await GetApi(options).UpdateUserById(serviceRoleToken, userId, userData);
		}

		/// <inheritdoc />
		public async Task<bool> DeleteUser(string uid, string serviceRoleToken, StatelessClientOptions options)
		{
			var result = await GetApi(options).DeleteUser(uid, serviceRoleToken);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public async Task<Session?> RefreshToken(string accessToken, string refreshToken, StatelessClientOptions options) => 
			await GetApi(options).RefreshAccessToken(accessToken, refreshToken);

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
