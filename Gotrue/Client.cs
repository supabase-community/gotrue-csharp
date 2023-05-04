﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
	/// <summary>
	/// The Gotrue Instance
	/// </summary>
	/// <example>
	/// var client = new Supabase.Gotrue.Client(options);
	/// var user = await client.SignIn("user@email.com", "fancyPassword");
	/// </example>
	public class Client : IGotrueClient<User, Session>
	{

		private DebugNotification? _debugNotification;

		public void AddDebugListener(Action<string, Exception?> listener)
		{
			_debugNotification ??= new DebugNotification();
			_debugNotification.AddDebugListener(listener);
		}

		/// <summary>
		/// Function that can be set to return dynamic headers.
		/// 
		/// Headers specified in the client options will ALWAYS take precedence over headers returned by this function.
		/// </summary>

		public Func<Dictionary<string, string>>? GetHeaders
		{
			get => _getHeaders;
			set
			{
				_getHeaders = value;

				if (_api != null)
					_api.GetHeaders = value;
			}
		}
		private Func<Dictionary<string, string>>? _getHeaders;

		/// <summary>
		/// Event Handler that raises an event when a user signs in, signs out, recovers password, or updates their record.
		/// </summary>
		public event EventHandler<ClientStateChanged>? StateChanged;

		/// <summary>
		/// The current User
		/// </summary>
		public User? CurrentUser { get; private set; }

		/// <summary>
		/// The current Session
		/// </summary>
		public Session? CurrentSession { get; private set; }

		/// <summary>
		/// Should Client Refresh Token Automatically? (via <see cref="ClientOptions"/>)
		/// </summary>
		protected bool AutoRefreshToken { get; private set; }

		/// <summary>
		/// Should Client Persist Session? (via <see cref="ClientOptions"/>)
		/// </summary>
		protected bool ShouldPersistSession { get; private set; }

		/// <summary>
		/// User defined function (via <see cref="ClientOptions"/>) to persist the session.
		/// </summary>
		// ReSharper disable once IdentifierTypo
		protected Func<Session, Task<bool>> SessionPersistor { get; private set; }

		/// <summary>
		/// User defined function (via <see cref="ClientOptions"/>) to retrieve the session.
		/// </summary>
		protected Func<Task<Session?>> SessionRetriever { get; private set; }

		/// <summary>
		/// User defined function (via <see cref="ClientOptions"/>) to destroy the session.
		/// </summary>
		protected Func<Task<bool>> SessionDestroyer { get; private set; }

		/// <summary>
		/// The initialized client options.
		/// </summary>
		internal ClientOptions<Session> Options { get; private set; }

		/// <summary>
		/// Internal timer reference for Refreshing Tokens (<see cref="AutoRefreshToken"/>)
		/// </summary>
		private Timer? _refreshTimer;

		private IGotrueApi<User, Session> _api;

		/// <summary>
		/// Initializes the Client. 
		/// 
		/// Although options are ... optional, you will likely want to at least specify a <see cref="ClientOptions.Url"/>.
		/// 
		/// Sessions are no longer automatically retrieved on construction, if you want to set the session, <see cref="RetrieveSessionAsync"/>
		/// 
		/// </summary>
		/// <param name="options"></param>
		public Client(ClientOptions<Session>? options = null)
		{
			if (options == null)
				options = new ClientOptions<Session>();

			Options = options;
			AutoRefreshToken = options.AutoRefreshToken;
			ShouldPersistSession = options.PersistSession;
			SessionPersistor = options.SessionPersistor;
			SessionRetriever = options.SessionRetriever;
			SessionDestroyer = options.SessionDestroyer;

			_api = new Api(options.Url, options.Headers);
		}

		/// <summary>
		/// Signs up a user by email address
		/// </summary>
		/// <remarks>
		/// By default, the user needs to verify their email address before logging in. To turn this off, disable Confirm email in your project.
		/// Confirm email determines if users need to confirm their email address after signing up.
		///     - If Confirm email is enabled, a user is returned but session is null.
		///     - If Confirm email is disabled, both a user and a session are returned.
		/// When the user confirms their email address, they are redirected to the SITE_URL by default. You can modify your SITE_URL or add additional redirect URLs in your project.
		/// If signUp() is called for an existing confirmed user:
		///     - If Confirm email is enabled in your project, an obfuscated/fake user object is returned.
		///     - If Confirm email is disabled, the error message, User already registered is returned.
		/// To fetch the currently logged-in user, refer to <see cref="User"/>.
		/// </remarks>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="options">Object containing redirectTo and optional user metadata (data)</param>
		/// <returns></returns>
		public Task<Session?> SignUp(string email, string password, SignUpOptions? options = null) => SignUp(SignUpType.Email, email, password, options);

		/// <summary>
		/// Signs up a user
		/// </summary>
		/// <remarks>
		/// By default, the user needs to verify their email address before logging in. To turn this off, disable Confirm email in your project.
		/// Confirm email determines if users need to confirm their email address after signing up.
		///     - If Confirm email is enabled, a user is returned but session is null.
		///     - If Confirm email is disabled, both a user and a session are returned.
		/// When the user confirms their email address, they are redirected to the SITE_URL by default. You can modify your SITE_URL or add additional redirect URLs in your project.
		/// If signUp() is called for an existing confirmed user:
		///     - If Confirm email is enabled in your project, an obfuscated/fake user object is returned.
		///     - If Confirm email is disabled, the error message, User already registered is returned.
		/// To fetch the currently logged-in user, refer to <see cref="User"/>.
		/// </remarks>
		/// <param name="type"></param>
		/// <param name="identifier"></param>
		/// <param name="password"></param>
		/// <param name="options">Object containing redirectTo and optional user metadata (data)</param>
		/// <returns></returns>
		public async Task<Session?> SignUp(SignUpType type, string identifier, string password, SignUpOptions? options = null)
		{
			await DestroySession();

			try
			{
				Session? session = null;
				switch (type)
				{
					case SignUpType.Email:
						session = await _api.SignUpWithEmail(identifier, password, options);
						break;
					case SignUpType.Phone:
						session = await _api.SignUpWithPhone(identifier, password, options);
						break;
				}

				if (session?.User?.ConfirmedAt != null || (session?.User != null && Options.AllowUnconfirmedUserSessions))
				{
					await PersistSession(session);

					StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

					return CurrentSession;
				}

				return session;
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
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> SignIn(string email, SignInOptions? options = null)
		{
			await DestroySession();

			try
			{
				await _api.SendMagicLinkEmail(email, options);
				return true;
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Allows signing in with an ID token issued by certain supported providers.
		/// The [idToken] is verified for validity and a new session is established.
		/// This method of signing in only supports [Provider.Google] or [Provider.Apple].
		/// </summary>
		/// <param name="provider">A supported provider (Google, Apple)</param>
		/// <param name="idToken">Provided from External Library</param>
		/// <param name="nonce">Provided from External Library</param>
		/// <param name="captchaToken">Provided from External Library</param>
		/// <returns></returns>
		/// <exception cref="InvalidProviderException"></exception>
		public async Task<Session?> SignInWithIdToken(Provider provider, string idToken, string? nonce = null, string? captchaToken = null)
		{
			try
			{
				await DestroySession();
				var result = await _api.SignInWithIdToken(provider, idToken, nonce, captchaToken);

				if (result != null)
					await PersistSession(result);

				return result;
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Log in a user using magiclink or a one-time password (OTP).
		/// 
		/// If the `{{ .ConfirmationURL }}` variable is specified in the email template, a magiclink will be sent.
		/// If the `{{ .Token }}` variable is specified in the email template, an OTP will be sent.
		/// If you're using phone sign-ins, only an OTP will be sent. You won't be able to send a magiclink for phone sign-ins.
		/// 
		/// Be aware that you may get back an error message that will not distinguish
		/// between the cases where the account does not exist or, that the account
		/// can only be accessed via social login.
		/// 
		/// Do note that you will need to configure a Whatsapp sender on Twilio
		/// if you are using phone sign in with the 'whatsapp' channel. The whatsapp
		/// channel is not supported on other providers at this time.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessEmailOptions options)
		{
			try
			{
				await DestroySession();
				return await _api.SignInWithOtp(options);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Log in a user using magiclink or a one-time password (OTP).
		/// 
		/// If the `{{ .ConfirmationURL }}` variable is specified in the email template, a magiclink will be sent.
		/// If the `{{ .Token }}` variable is specified in the email template, an OTP will be sent.
		/// If you're using phone sign-ins, only an OTP will be sent. You won't be able to send a magiclink for phone sign-ins.
		/// 
		/// Be aware that you may get back an error message that will not distinguish
		/// between the cases where the account does not exist or, that the account
		/// can only be accessed via social login.
		/// 
		/// Do note that you will need to configure a Whatsapp sender on Twilio
		/// if you are using phone sign in with the 'whatsapp' channel. The whatsapp
		/// channel is not supported on other providers at this time.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessPhoneOptions options)
		{
			try
			{
				await DestroySession();
				return await _api.SignInWithOtp(options);
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
		/// <param name="options"></param>
		/// <returns></returns>
		public Task<bool> SendMagicLink(string email, SignInOptions? options = null) => SignIn(email, options);


		/// <summary>
		/// Signs in a User.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Task<Session?> SignIn(string email, string password) => SignIn(SignInType.Email, email, password);

		/// <summary>
		/// Log in an existing user with an email and password or phone and password.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Task<Session?> SignInWithPassword(string email, string password) => SignIn(email, password);

		/// <summary>
		/// Log in an existing user, or login via a third-party provider.
		/// </summary>
		/// <param name="type">Type of Credentials being passed</param>
		/// <param name="identifierOrToken">An email, phone, or RefreshToken</param>
		/// <param name="password">Password to account (optional if `RefreshToken`)</param>
		/// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
		/// <returns></returns>
		public async Task<Session?> SignIn(SignInType type, string identifierOrToken, string? password = null, string? scopes = null)
		{
			await DestroySession();

			try
			{
				Session? session = null;
				switch (type)
				{
					case SignInType.Email:
						session = await _api.SignInWithEmail(identifierOrToken, password!);
						break;
					case SignInType.Phone:
						if (string.IsNullOrEmpty(password))
						{
							await _api.SendMobileOTP(identifierOrToken);
							return null;
						}

						session = await _api.SignInWithPhone(identifierOrToken, password!);
						break;
					case SignInType.RefreshToken:
						CurrentSession = new Session();
						CurrentSession.RefreshToken = identifierOrToken;

						await RefreshToken();

						return CurrentSession;
				}

				if (session?.User?.ConfirmedAt != null || (session?.User != null && Options.AllowUnconfirmedUserSessions))
				{
					await PersistSession(session);
					StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
					return CurrentSession;
				}

				return null;
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Retrieves a <see cref="ProviderAuthState"/> to redirect to for signing in with a <see cref="Provider"/>.
		///
		/// This will likely be paired with a PKCE flow (set in SignInOptions) - after redirecting the
		/// user to the flow, you should pair with <see cref="ExchangeCodeForSession(string, string)"/>
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<ProviderAuthState> SignIn(Provider provider, SignInOptions? options = null)
		{
			await DestroySession();

			var providerUri = _api.GetUriForProvider(provider, options);
			return providerUri;
		}

		/// <summary>
		/// Log in a user given a User supplied OTP received via mobile.
		/// </summary>
		/// <param name="phone">The user's phone number.</param>
		/// <param name="token">Token sent to the user's phone.</param>
		/// <param name="type">SMS or phone change</param>
		/// <returns></returns>
		public async Task<Session?> VerifyOTP(string phone, string token, MobileOtpType type = MobileOtpType.SMS)
		{
			try
			{
				await DestroySession();

				var session = await _api.VerifyMobileOTP(phone, token, type);

				if (session?.AccessToken != null)
				{
					await PersistSession(session);
					StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
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
		/// Log in a user give a user supplied OTP received via email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="token"></param>
		/// <param name="type">Defaults to MagicLink</param>
		/// <returns></returns>
		public async Task<Session?> VerifyOTP(string email, string token, EmailOtpType type = EmailOtpType.MagicLink)
		{
			try
			{
				await DestroySession();

				var session = await _api.VerifyEmailOTP(email, token, type);

				if (session?.AccessToken != null)
				{
					await PersistSession(session);
					StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
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
		/// Signs out a user and invalidates the current token.
		/// </summary>
		/// <returns></returns>
		public async Task SignOut()
		{
			if (CurrentSession != null)
			{
				if (CurrentSession.AccessToken != null)
					await _api.SignOut(CurrentSession.AccessToken);

				_refreshTimer?.Dispose();

				await DestroySession();

				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedOut));
			}
		}

		/// <summary>
		/// Updates a User.
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public async Task<User?> Update(UserAttributes attributes)
		{
			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
				throw new Exception("Not Logged in.");

			try
			{
				var result = await _api.UpdateUser(CurrentSession.AccessToken!, attributes);

				CurrentUser = result;

				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.UserUpdated));

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
		public async Task<bool> InviteUserByEmail(string email, string jwt)
		{
			try
			{
				var response = await _api.InviteUserByEmail(email, jwt);
				response.ResponseMessage?.EnsureSuccessStatusCode();
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
		public async Task<bool> DeleteUser(string uid, string jwt)
		{
			try
			{
				var result = await _api.DeleteUser(uid, jwt);
				result.ResponseMessage?.EnsureSuccessStatusCode();
				return true;
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
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
		public async Task<UserList<User>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null,
			int? perPage = null)
		{
			try
			{
				return await _api.ListUsers(jwt, filter, sortBy, sortOrder, page, perPage);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Get User details by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<User?> GetUserById(string jwt, string userId)
		{
			try
			{
				return await _api.GetUserById(jwt, userId);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Get User details by JWT. Can be used to validate a JWT.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a JWT that originates from a user.</param>
		/// <returns></returns>
		public async Task<User?> GetUser(string jwt)
		{
			try
			{
				return await _api.GetUser(jwt);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Create a user (as a service_role)
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public Task<User?> CreateUser(string jwt, string email, string password, AdminUserAttributes? attributes = null)
		{
			if (attributes == null)
			{
				attributes = new AdminUserAttributes();
			}
			attributes.Email = email;
			attributes.Password = password;

			return CreateUser(jwt, attributes);
		}

		/// <summary>
		/// Create a user (as a service_role)
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public async Task<User?> CreateUser(string jwt, AdminUserAttributes attributes)
		{
			try
			{
				return await _api.CreateUser(jwt, attributes);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Update user by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public async Task<User?> UpdateUserById(string jwt, string userId, AdminUserAttributes userData)
		{
			try
			{
				return await _api.UpdateUserById(jwt, userId, userData);
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Sends a reset request to an email address.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public async Task<bool> ResetPasswordForEmail(string email)
		{
			try
			{
				var result = await _api.ResetPasswordForEmail(email);
				result.ResponseMessage?.EnsureSuccessStatusCode();
				return true;
			}
			catch (RequestException ex)
			{
				throw ExceptionHandler.Parse(ex);
			}
		}

		/// <summary>
		/// Refreshes the currently logged in User's Session.
		/// </summary>
		/// <returns></returns>
		public async Task<Session?> RefreshSession()
		{
			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
				throw new Exception("Not Logged in.");

			await RefreshToken();

			var user = await _api.GetUser(CurrentSession.AccessToken!);
			CurrentUser = user;

			return CurrentSession;
		}

		/// <summary>
		///  Overrides the JWT on the current session. The JWT will then be sent in all subsequent network requests.
		/// </summary>
		/// <param name="accessToken">The JWT access token.</param>
		/// <returns>Session.</returns>
		public Session SetAuth(string accessToken)
		{
			if (CurrentSession == null) CurrentSession = new Session();

			CurrentSession.AccessToken = accessToken;
			CurrentSession.TokenType = "bearer";
			CurrentSession.User = CurrentUser;

			StateChanged?.Invoke(this, new ClientStateChanged(AuthState.TokenRefreshed));
			return CurrentSession;
		}

		/// <summary>
		/// Parses a <see cref="Session"/> out of a <see cref="Uri"/>'s Query parameters.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="storeSession"></param>
		/// <returns></returns>
		public async Task<Session?> GetSessionFromUrl(Uri uri, bool storeSession = true)
		{
			var query = string.IsNullOrEmpty(uri.Fragment) ? HttpUtility.ParseQueryString(uri.Query) : HttpUtility.ParseQueryString('?' + uri.Fragment.TrimStart('#'));

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

			var user = await _api.GetUser(accessToken);

			var session = new Session
			{
				AccessToken = accessToken,
				ExpiresIn = int.Parse(expiresIn),
				RefreshToken = refreshToken,
				TokenType = tokenType,
				User = user
			};

			if (storeSession)
			{
				await PersistSession(session);
				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

				if (query.Get("type") == "recovery")
					StateChanged?.Invoke(this, new ClientStateChanged(AuthState.PasswordRecovery));
			}

			return session;
		}

		/// <summary>
		/// Retrieves the Session by calling <see cref="SessionRetriever"/> - sets internal state and timers.
		/// </summary>
		/// <returns></returns>
		public async Task<Session?> RetrieveSessionAsync()
		{
			if (SessionRetriever == null) return null;

			var session = await SessionRetriever.Invoke();

			if (session != null && session.ExpiresAt() < DateTime.Now)
			{
				if (AutoRefreshToken && session.RefreshToken != null)
				{
					try
					{
						await RefreshToken(session.RefreshToken);
						return CurrentSession;
					}
					catch
					{
						await DestroySession();
						return null;
					}
				}
				else
				{
					await DestroySession();
					return null;
				}
			}
			else if (session == null || session.User == null)
			{
				_debugNotification?.Log("Stored Session is missing data.");
				await DestroySession();
				return null;
			}
			else
			{
				CurrentSession = session;
				CurrentUser = session.User;

				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

				InitRefreshTimer();

				return CurrentSession;
			}
		}

		/// <summary>
		/// Logs in an existing user via a third-party provider.
		/// </summary>
		/// <param name="codeVerifier"></param>
		/// <param name="authCode"></param>
		public async Task<Session?> ExchangeCodeForSession(string codeVerifier, string authCode)
		{
			var result = await _api.ExchangeCodeForSession(codeVerifier, authCode);

			if (result != null)
			{
				await PersistSession(result);
				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
				return CurrentSession;
			}

			return null;
		}

		/// <summary>
		/// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionPersistor"/>
		/// </summary>
		/// <param name="session"></param>
		private async Task PersistSession(Session session)
		{
			CurrentSession = session;
			CurrentUser = session.User;

			var expiration = session.ExpiresIn;

			if (AutoRefreshToken && expiration != default)
				InitRefreshTimer();

			if (ShouldPersistSession && SessionPersistor != null)
				await SessionPersistor.Invoke(session);
		}

		/// <summary>
		/// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionDestroyer"/>
		/// </summary>
		private async Task DestroySession()
		{
			CurrentSession = null;
			CurrentUser = null;

			if (ShouldPersistSession && SessionDestroyer != null)
				await SessionDestroyer.Invoke();
		}

		/// <summary>
		/// Refreshes a Token
		/// </summary>
		/// <returns></returns>
		private async Task RefreshToken(string? refreshToken = null)
		{
			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession?.RefreshToken) && string.IsNullOrEmpty(refreshToken))
				throw new Exception("No current session.");

			refreshToken ??= CurrentSession!.RefreshToken;

			var result = await _api.RefreshAccessToken(refreshToken!);

			if (result == null || string.IsNullOrEmpty(result.AccessToken))
				throw new Exception("Could not refresh token from provided session.");

			CurrentSession = result;
			CurrentUser = result.User;

			if (ShouldPersistSession && SessionPersistor != null)
				await SessionPersistor.Invoke(result);

			StateChanged?.Invoke(this, new ClientStateChanged(AuthState.TokenRefreshed));
			StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

			if (AutoRefreshToken && CurrentSession.ExpiresIn != default)
				InitRefreshTimer();
		}

		private void InitRefreshTimer()
		{
			if (CurrentSession == null || CurrentSession.ExpiresIn == default) return;

			if (_refreshTimer != null)
				_refreshTimer.Dispose();

			try
			{
				// Interval should be t - (1/5(n)) (i.e. if session time (t) 3600s, attempt refresh at 2880s or 720s (1/5) seconds before expiration)
				int interval = (int)Math.Floor(CurrentSession.ExpiresIn * 4.0f / 5.0f);
				int timeoutSeconds = Convert.ToInt32((CurrentSession.CreatedAt.AddSeconds(interval) - DateTime.Now).TotalSeconds);
				TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);

				_refreshTimer = new Timer(HandleRefreshTimerTick, null, timeout, Timeout.InfiniteTimeSpan);
			}
			catch
			{
				_debugNotification?.Log("Unable to parse session timestamp, refresh timer will not work. If persisting, open issue on Github");
			}
		}

		private async void HandleRefreshTimerTick(object _)
		{
			_refreshTimer?.Dispose();

			try
			{
				// Will re-init the refresh timer on success, on failure, stops refreshing timer.
				await RefreshToken();
			}
			catch (HttpRequestException ex)
			{
				// The request failed - potential network error?
				_debugNotification?.Log(ex.Message, ex);
				_refreshTimer = new Timer(HandleRefreshTimerTick, null, 5000, -1);
			}
			catch (Exception ex)
			{
				_debugNotification?.Log(ex.Message, ex);
				StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedOut));
			}
		}
	}

	/// <summary>
	/// Class representing a state change on the <see cref="Client"/>.
	/// </summary>
	public class ClientStateChanged : EventArgs
	{
		public AuthState State { get; private set; }

		public ClientStateChanged(AuthState state)
		{
			State = state;
		}
	}
}
