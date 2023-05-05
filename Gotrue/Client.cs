﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.Constants.AuthState;

namespace Supabase.Gotrue
{
	/// <summary>
	/// The Gotrue Instance
	/// </summary>
	/// <example>
	/// var client = new Supabase.Gotrue.Client(options);
	/// var user = await client.SignIn("user@email.com", "fancyPassword");
	/// </example>
	[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
	public class Client : IGotrueClient<User, Session>
	{

		private DebugNotification? _debugNotification;

		private readonly List<IGotrueClient<User, Session>.AuthEventHandler> _authEventHandlers = new List<IGotrueClient<User, Session>.AuthEventHandler>();

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

		public void NotifyStateChange(AuthState stateChanged)
		{
			foreach (var handler in _authEventHandlers)
			{
				handler.Invoke(this, stateChanged);
			}
		}

		/// <summary>
		/// The current User
		/// </summary>
		public User? CurrentUser { get; private set; }

		public void AddStateChangedListener(IGotrueClient<User, Session>.AuthEventHandler authEventHandler)
		{
			if (_authEventHandlers.Contains(authEventHandler))
				return;

			_authEventHandlers.Add(authEventHandler);

		}
		public void RemoveStateChangedListener(IGotrueClient<User, Session>.AuthEventHandler authEventHandler)
		{
			if (!_authEventHandlers.Contains(authEventHandler))
				return;

			_authEventHandlers.Remove(authEventHandler);
		}
		public void ClearStateChangedListeners()
		{
			_authEventHandlers.Clear();
		}

		/// <summary>
		/// The current Session
		/// </summary>
		public Session? CurrentSession { get; private set; }

		/// <summary>
		/// The initialized client options.
		/// </summary>
		public ClientOptions Options { get; }

		/// <summary>
		/// Internal timer reference for Refreshing Tokens (<see>
		///     <cref>AutoRefreshToken</cref>
		/// </see>
		/// )
		/// </summary>
		private Timer? _refreshTimer;

		private readonly IGotrueApi<User, Session> _api;

		/// <summary>
		/// Initializes the Client. 
		/// 
		/// Although options are ... optional, you will likely want to at least specify a <see>
		///     <cref>ClientOptions.Url</cref>
		/// </see>
		/// .
		/// 
		/// Sessions are no longer automatically retrieved on construction, if you want to set the session, <see cref="RetrieveSessionAsync"/>
		/// 
		/// </summary>
		/// <param name="options"></param>
		public Client(ClientOptions? options = null)
		{
			options ??= new ClientOptions();

			Options = options;

			if (options.PersistSession)
			{
				var persistenceListener = new PersistenceListener(options.SessionPersistor, options.SessionDestroyer, options.SessionRetriever);
				_authEventHandlers.Add(persistenceListener.EventHandler);
			}

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
		/// To fetch the currently logged-in user, refer to <see>
		///     <cref>User</cref>
		/// </see>
		/// .
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
			DestroySession();

			var session = type switch
			{
				SignUpType.Email => await _api.SignUpWithEmail(identifier, password, options),
				SignUpType.Phone => await _api.SignUpWithPhone(identifier, password, options),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};

			if (session?.User?.ConfirmedAt != null || session?.User != null && Options.AllowUnconfirmedUserSessions)
			{
				UpdateSession(session);
				NotifyStateChange(SignedIn);
				return CurrentSession;
			}

			return session;
		}


		/// <summary>
		/// Sends a Magic email login link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<bool> SendMagicLinkEmail(string email, SignInOptions? options = null)
		{
			await _api.SendMagicLinkEmail(email, options);
			return true;
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
		/// <exception>
		///     <cref>InvalidProviderException</cref>
		/// </exception>
		public async Task<Session?> SignInWithIdToken(Provider provider, string idToken, string? nonce = null, string? captchaToken = null)
		{
			NotifyStateChange(SignedOut);
			var result = await _api.SignInWithIdToken(provider, idToken, nonce, captchaToken);

			if (result != null)
				NotifyStateChange(SignedIn);

			return result;
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
			NotifyStateChange(SignedOut);
			return await _api.SignInWithOtp(options);
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
			NotifyStateChange(SignedOut);
			return await _api.SignInWithOtp(options);
		}

		/// <summary>
		/// Sends a Magic email login link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public Task<bool> SendMagicLink(string email, SignInOptions? options = null) => SendMagicLinkEmail(email, options);

		/// <summary>
		/// Signs in a User.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Task<Session?> SendMagicLinkEmail(string email, string password) => SendMagicLinkEmail(SignInType.Email, email, password);

		/// <summary>
		/// Log in an existing user with an email and password or phone and password.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Task<Session?> SignInWithPassword(string email, string password) => SendMagicLinkEmail(email, password);

		/// <summary>
		/// Log in an existing user, or login via a third-party provider.
		/// </summary>
		/// <param name="type">Type of Credentials being passed</param>
		/// <param name="identifierOrToken">An email, phone, or RefreshToken</param>
		/// <param name="password">Password to account (optional if `RefreshToken`)</param>
		/// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
		/// <returns></returns>
		public async Task<Session?> SendMagicLinkEmail(SignInType type, string identifierOrToken, string? password = null, string? scopes = null)
		{
			Session? session;
			switch (type)
			{
				case SignInType.Email:
					session = await _api.SignInWithEmail(identifierOrToken, password!);
					UpdateSession(session);
					break;
				case SignInType.Phone:
					if (string.IsNullOrEmpty(password))
					{
						await _api.SendMobileOTP(identifierOrToken);
						return null;
					}

					session = await _api.SignInWithPhone(identifierOrToken, password!);
					UpdateSession(session);
					break;
				case SignInType.RefreshToken:
					CurrentSession = new Session();
					CurrentSession.RefreshToken = identifierOrToken;
					await RefreshToken();
					return CurrentSession;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			if (session?.User?.ConfirmedAt != null || session?.User != null && Options.AllowUnconfirmedUserSessions)
			{
				NotifyStateChange(SignedIn);
				return CurrentSession;
			}

			return null;
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
		public Task<ProviderAuthState> SendMagicLinkEmail(Provider provider, SignInOptions? options = null)
		{
			DestroySession();

			var providerUri = _api.GetUriForProvider(provider, options);
			return Task.FromResult(providerUri);
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
			DestroySession();

			var session = await _api.VerifyMobileOTP(phone, token, type);

			if (session?.AccessToken != null)
			{
				UpdateSession(session);
				NotifyStateChange(SignedIn);
				return session;
			}

			return null;
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
			DestroySession();

			var session = await _api.VerifyEmailOTP(email, token, type);

			if (session?.AccessToken != null)
			{
				UpdateSession(session);
				NotifyStateChange(SignedIn);
				return session;
			}

			return null;
		}

		/// <summary>
		/// Signs out a user and invalidates the current token.
		/// </summary>
		/// <returns></returns>
		public async Task SignOut()
		{
			if (CurrentSession?.AccessToken != null)
				await _api.SignOut(CurrentSession.AccessToken);
			_refreshTimer?.Dispose();
			UpdateSession(null);
			NotifyStateChange(SignedOut);
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

			var result = await _api.UpdateUser(CurrentSession.AccessToken!, attributes);
			CurrentUser = result;
			NotifyStateChange(UserUpdated);

			return result;
		}

		/// <summary>
		/// Sends an invite email link to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
		/// <returns></returns>
		public async Task<bool> InviteUserByEmail(string email, string jwt)
		{
			var response = await _api.InviteUserByEmail(email, jwt);
			response.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Deletes a User.
		/// </summary>
		/// <param name="uid"></param>
		/// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
		/// <returns></returns>
		public async Task<bool> DeleteUser(string uid, string jwt)
		{
			var result = await _api.DeleteUser(uid, jwt);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <summary>
		/// Lists users
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="filter">A string for example part of the email</param>
		/// <param name="sortBy">Snake case string of the given key, currently only created_at is supported</param>
		/// <param name="sortOrder">asc or desc, if null desc is used</param>
		/// <param name="page">page to show for pagination</param>
		/// <param name="perPage">items per page for pagination</param>
		/// <returns></returns>
		public async Task<UserList<User>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null,
			int? perPage = null)
		{
			return await _api.ListUsers(jwt, filter, sortBy, sortOrder, page, perPage);
		}

		/// <summary>
		/// Get User details by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<User?> GetUserById(string jwt, string userId)
		{

			return await _api.GetUserById(jwt, userId);

		}

		/// <summary>
		/// Get User details by JWT. Can be used to validate a JWT.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a JWT that originates from a user.</param>
		/// <returns></returns>
		public async Task<User?> GetUser(string jwt)
		{
			return await _api.GetUser(jwt);
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
			attributes ??= new AdminUserAttributes();
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
			return await _api.CreateUser(jwt, attributes);
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
			return await _api.UpdateUserById(jwt, userId, userData);
		}

		/// <summary>
		/// Sends a reset request to an email address.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public async Task<bool> ResetPasswordForEmail(string email)
		{
			var result = await _api.ResetPasswordForEmail(email);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
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
			CurrentSession ??= new Session();

			CurrentSession.AccessToken = accessToken;
			CurrentSession.TokenType = "bearer";
			CurrentSession.User = CurrentUser;

			NotifyStateChange(TokenRefreshed);
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
				UpdateSession(session);
				NotifyStateChange(SignedIn);

				if (query.Get("type") == "recovery")
					NotifyStateChange(PasswordRecovery);
			}

			return session;
		}

		/// <summary>
		/// Retrieves the Session by calling <see>
		///     <cref>SessionRetriever</cref>
		/// </see>
		/// - sets internal state and timers.
		/// </summary>
		/// <returns></returns>
		public async Task<Session?> RetrieveSessionAsync()
		{
			var session = CurrentSession;

			if (session != null && session.ExpiresAt() < DateTime.Now)
			{
				if (Options.AutoRefreshToken && session.RefreshToken != null)
				{
					try
					{
						await RefreshToken(session.RefreshToken);
						return CurrentSession;
					}
					catch
					{
						DestroySession();
						return null;
					}
				}
				DestroySession();
				return null;
			}
			if (session?.User == null)
			{
				_debugNotification?.Log("Stored Session is missing data.");
				DestroySession();
				return null;
			}
			CurrentSession = session;
			CurrentUser = session.User;

			NotifyStateChange(SignedIn);

			InitRefreshTimer();

			return CurrentSession;
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
				UpdateSession(result);
				NotifyStateChange(SignedIn);
				return CurrentSession;
			}

			return null;
		}

		/// <summary>
		/// Saves the session
		/// </summary>
		/// <param name="session"></param>
		private void UpdateSession(Session? session)
		{
			if (session == null)
			{
				CurrentSession = null;
				CurrentUser = null;
				NotifyStateChange(SignedOut);
				return;
			}

			var dirty = CurrentSession != session;

			CurrentSession = session;
			CurrentUser = session.User;

			var expiration = session.ExpiresIn;

			if (Options.AutoRefreshToken && expiration != default)
				InitRefreshTimer();

			if (dirty)
				NotifyStateChange(UserUpdated);
		}

		/// <summary>
		/// Clears the session
		/// </summary>
		private void DestroySession()
		{
			UpdateSession(null);
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

			NotifyStateChange(TokenRefreshed);

			if (Options.AutoRefreshToken && CurrentSession.ExpiresIn != default)
				InitRefreshTimer();
		}

		private void InitRefreshTimer()
		{
			if (CurrentSession == null || CurrentSession.ExpiresIn == default) return;

			_refreshTimer?.Dispose();

			try
			{
				// Interval should be t - (1/5(n)) (i.e. if session time (t) 3600s, attempt refresh at 2880s or 720s (1/5) seconds before expiration)
				var interval = (int)Math.Floor(CurrentSession.ExpiresIn * 4.0f / 5.0f);
				var timeoutSeconds = Convert.ToInt32((CurrentSession.CreatedAt.AddSeconds(interval) - DateTime.Now).TotalSeconds);
				var timeout = TimeSpan.FromSeconds(timeoutSeconds);

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
				NotifyStateChange(SignedOut);
			}
		}
	}
}
