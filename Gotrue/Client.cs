using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.Constants.AuthState;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace Supabase.Gotrue
{
	/// <summary>
	/// GoTrue stateful Client.
	///
	/// This class is best used as a long-lived singleton object in your application. You can attach listeners
	/// to be notified of changes to the user log in state, a persistence system for sessions across application
	/// launches, and more. It includes a (optional, on by default) background thread that runs to refresh the
	/// user's session token.
	///
	/// Check out the test suite for examples of use.
	/// </summary>
	/// <example>
	/// var client = new Supabase.Gotrue.Client(options);
	/// var user = await client.SignIn("user@email.com", "fancyPassword");
	/// </example>
	public class Client : IGotrueClient<User, Session>
	{
		/// <summary>
		/// The underlying API requests object that sends the requests
		/// </summary>
		private readonly IGotrueApi<User, Session> _api;

		/// <summary>
		/// Handlers for notifications of state changes.
		/// </summary>
		private readonly List<IGotrueClient<User, Session>.AuthEventHandler> _authEventHandlers = new List<IGotrueClient<User, Session>.AuthEventHandler>();

		/// <summary>
		/// Gets notifications if there is a failure not visible by exceptions (e.g. background thread refresh failure)
		/// </summary>
		private DebugNotification? _debugNotification;

		/// <summary>
		/// Object called to persist the session (e.g. filesystem or cookie)
		/// </summary>
		private IGotruePersistenceListener<Session>? _sessionPersistence;

		/// <summary>
		/// Initializes the GoTrue stateful client. 
		/// 
		/// You will likely want to at least specify a <see>
		///     <cref>ClientOptions.Url</cref>
		/// </see>
		/// 
		/// Sessions are not automatically retrieved when this object is created.
		/// 
		/// If you want to load the session from your persistence store, <see>
		///     <cref>GotrueSessionPersistence</cref>
		/// </see>.
		///
		/// If you want to load/refresh the session, <see>
		///     <cref>RetrieveSessionAsync</cref>
		/// </see>.
		///
		/// For a typical client application, you'll want to load the session from persistence
		/// and then refresh it. If your application is listening for session changes, you'll
		/// get two SignIn notifications if the persisted session is valid - one for the
		/// session loaded from disk, and a second on a successful session refresh.
		/// 
		/// <remarks></remarks>
		/// <example>
		///		var client = new Supabase.Gotrue.Client(options);
		///     client.LoadSession();
		///		await client.RetrieveSessionAsync();
		/// </example>
		/// </summary>
		/// <param name="options"></param>
		public Client(ClientOptions? options = null)
		{
			options ??= new ClientOptions();
			Options = options;
			_api = new Api(options.Url, options.Headers);

			if (options.AutoRefreshToken)
			{
				_authEventHandlers.Add(new TokenRefresh(this).ManageAutoRefresh);
			}
		}

		/// <summary>
		/// Set the Session persistence system. Typically an application specific file system location.
		/// </summary>
		/// <param name="persistence"></param>
		public void SetPersistence(IGotrueSessionPersistence<Session> persistence)
		{
			if (_sessionPersistence != null) _authEventHandlers.Remove(_sessionPersistence.EventHandler);
			_sessionPersistence = new PersistenceListener(persistence);
			_authEventHandlers.Add(_sessionPersistence.EventHandler);
		}

		/// <summary>
		/// The initialized client options.
		/// </summary>
		public ClientOptions Options { get; }

		/// <summary>
		/// Notifies all listeners that the current user auth state has changed.
		///
		/// This is mainly used internally to fire notifications - most client applications won't need this.
		/// </summary>
		/// <param name="stateChanged"></param>
		public void NotifyAuthStateChange(AuthState stateChanged)
		{
			foreach (var handler in _authEventHandlers)
			{
				try
				{
					handler.Invoke(this, stateChanged);
				}
				catch (Exception e)
				{
					_debugNotification?.Log("Auth State Change Handler Failure", e);
				}
			}
		}

		/// <summary>
		/// The currently logged in User. This is a local cache of the current session User. 
		/// To persist modifications to the User you'll want to use other methods.
		/// <see cref="Update"/>>
		/// </summary>
		public User? CurrentUser { get => CurrentSession?.User; }

		/// <summary>
		/// Adds a listener to be notified when the user state changes (e.g. the user logs in, logs out,
		/// the token is refreshed, etc).
		///
		/// <see cref="AuthState"/>
		/// </summary>
		/// <param name="authEventHandler"></param>
		public void AddStateChangedListener(IGotrueClient<User, Session>.AuthEventHandler authEventHandler)
		{
			if (_authEventHandlers.Contains(authEventHandler)) return;

			_authEventHandlers.Add(authEventHandler);
		}

		/// <summary>
		/// Removes a specified listener from event state changes.
		/// </summary>
		/// <param name="authEventHandler"></param>
		public void RemoveStateChangedListener(IGotrueClient<User, Session>.AuthEventHandler authEventHandler)
		{
			if (!_authEventHandlers.Contains(authEventHandler)) return;

			_authEventHandlers.Remove(authEventHandler);
		}

		/// <summary>
		/// Clears all of the listeners from receiving event state changes.
		///
		/// WARNING: The persistence handler and refresh token thread are installed as state change
		/// listeners. Clearing the listeners will also delete these handlers.
		/// </summary>
		public void ClearStateChangedListeners()
		{
			_authEventHandlers.Clear();
		}

		/// <inheritdoc />
		public bool Online { get; set; } = true;

		/// <summary>
		/// The current Session as managed by this client. Does not refresh tokens or have any other side effects.
		///
		/// You probably don't want to directly make changes to this object - you'll want to use other methods
		/// on this class to make changes.
		/// </summary>
		public Session? CurrentSession { get; private set; }

		/// <summary>
		/// Signs up a user by email address.
		/// </summary>
		/// <remarks>
		/// By default, the user needs to verify their email address before logging in. To turn this off, disable Confirm email in your project.
		/// Confirm email determines if users need to confirm their email address after signing up.
		///     - If Confirm email is enabled, a user is returned but session is null.
		///     - If Confirm email is disabled, both a user and a session are returned.
		/// When the user confirms their email address, they are redirected to the SITE_URL by default. You can modify your SITE_URL or
		/// add additional redirect URLs in your project.
		/// If signUp() is called for an existing confirmed user:
		///     - If Confirm email is enabled in your project, an obfuscated/fake user object is returned.
		///     - If Confirm email is disabled, the error message, User already registered is returned.
		/// To fetch the currently logged-in user, refer to <see>
		///     <cref>User</cref>
		/// </see>.
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
		/// Calling this method will log out the current user session (if any).
		/// 
		/// By default, the user needs to verify their email address before logging in. To turn this off, disable confirm email in your project.
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
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

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
				NotifyAuthStateChange(SignedIn);
				return CurrentSession;
			}

			return session;
		}


		/// <summary>
		/// Sends a magic link login email to the specified email.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="options"></param>
		public async Task<bool> SignIn(string email, SignInOptions? options = null)
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

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
		/// <remarks>Calling this method will eliminate the current session (if any).</remarks>
		/// <exception>
		///     <cref>InvalidProviderException</cref>
		/// </exception>
		public async Task<Session?> SignInWithIdToken(Provider provider, string idToken, string? nonce = null, string? captchaToken = null)
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			DestroySession();
			var result = await _api.SignInWithIdToken(provider, idToken, nonce, captchaToken);

			if (result != null) NotifyAuthStateChange(SignedIn);

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
		/// <remarks>Calling this method will wipe out the current session (if any)</remarks>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessEmailOptions options)
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			DestroySession();
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
		/// <remarks>Calling this method will wipe out the current session (if any)</remarks>
		/// <param name="options"></param>
		/// <returns></returns>
		public async Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessPhoneOptions options)
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			DestroySession();
			return await _api.SignInWithOtp(options);
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
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			Session? newSession;
			switch (type)
			{
				case SignInType.Email:
					newSession = await _api.SignInWithEmail(identifierOrToken, password!);
					UpdateSession(newSession);
					break;
				case SignInType.Phone:
					if (string.IsNullOrEmpty(password))
					{
						await _api.SendMobileOTP(identifierOrToken);
						return null;
					}

					newSession = await _api.SignInWithPhone(identifierOrToken, password!);
					UpdateSession(newSession);
					break;
				case SignInType.RefreshToken:
					await RefreshToken(identifierOrToken);
					return CurrentSession;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			if (newSession?.User?.ConfirmedAt != null || newSession?.User != null && Options.AllowUnconfirmedUserSessions)
			{
				NotifyAuthStateChange(SignedIn);
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
		public Task<ProviderAuthState> SignIn(Provider provider, SignInOptions? options = null)
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

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
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			DestroySession();

			var session = await _api.VerifyMobileOTP(phone, token, type);

			if (session?.AccessToken != null)
			{
				UpdateSession(session);
				NotifyAuthStateChange(SignedIn);
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
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			DestroySession();

			var session = await _api.VerifyEmailOTP(email, token, type);

			if (session?.AccessToken != null)
			{
				UpdateSession(session);
				NotifyAuthStateChange(SignedIn);
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
			if (CurrentSession?.AccessToken != null) await _api.SignOut(CurrentSession.AccessToken);
			UpdateSession(null);
			NotifyAuthStateChange(SignedOut);
		}

		/// <summary>
		/// Updates a User.
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public async Task<User?> Update(UserAttributes attributes)
		{
			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
				throw new GotrueException("Not Logged in.");

			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			var result = await _api.UpdateUser(CurrentSession.AccessToken!, attributes);
			CurrentSession.User = result;
			NotifyAuthStateChange(UserUpdated);

			return result;
		}

		/// <summary>
		/// Used for re-authenticating a user in password changes.
		///
		/// See: https://github.com/supabase/gotrue#get-reauthenticate
		/// </summary>
		/// <returns></returns>
		/// <exception cref="GotrueException"></exception>
		public async Task<bool> Reauthenticate()
		{
			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
				throw new GotrueException("Not Logged in.", NoSessionFound);

			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			var response = await _api.Reauthenticate(CurrentSession.AccessToken!);

			return response.ResponseMessage?.IsSuccessStatusCode ?? false;
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
		public Task<UserList<User>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null,
			int? perPage = null) => _api.ListUsers(jwt, filter, sortBy, sortOrder, page, perPage);

		/// <summary>
		/// Get User details by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public Task<User?> GetUserById(string jwt, string userId) => _api.GetUserById(jwt, userId);

		/// <summary>
		/// Get User details by JWT. Can be used to validate a JWT.
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a JWT that originates from a user.</param>
		/// <returns></returns>
		public Task<User?> GetUser(string jwt) => _api.GetUser(jwt);

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
		public Task<User?> CreateUser(string jwt, AdminUserAttributes attributes) => _api.CreateUser(jwt, attributes);

		/// <summary>
		/// Update user by Id
		/// </summary>
		/// <param name="jwt">A valid JWT. Must be a full-access API key (e.g. service_role key).</param>
		/// <param name="userId"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public Task<User?> UpdateUserById(string jwt, string userId, AdminUserAttributes userData) => _api.UpdateUserById(jwt, userId, userData);

		/// <summary>
		/// Sends a reset request to an email address.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
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
				throw new GotrueException("Not Logged in.", NoSessionFound);

			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			await RefreshToken();

			var user = await _api.GetUser(CurrentSession.AccessToken!);
			CurrentSession.User = user;

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

			NotifyAuthStateChange(TokenRefreshed);
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

			if (!string.IsNullOrEmpty(errorDescription)) throw new GotrueException(errorDescription, BadSessionUrl);

			var accessToken = query.Get("access_token");

			if (string.IsNullOrEmpty(accessToken))
				throw new GotrueException("No access_token detected.", BadSessionUrl);

			var expiresIn = query.Get("expires_in");

			if (string.IsNullOrEmpty(expiresIn)) throw new GotrueException("No expires_in detected.", BadSessionUrl);

			var refreshToken = query.Get("refresh_token");

			if (string.IsNullOrEmpty(refreshToken))
				throw new GotrueException("No refresh_token detected.", BadSessionUrl);

			var tokenType = query.Get("token_type");

			if (string.IsNullOrEmpty(tokenType)) throw new GotrueException("No token_type detected.", BadSessionUrl);

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
				NotifyAuthStateChange(SignedIn);

				if (query.Get("type") == "recovery") NotifyAuthStateChange(PasswordRecovery);
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
			// No session, so just return.
			if (CurrentSession == null)
				return null;

			// Check to see if the session has expired. If so go ahead and destroy it.
			if (CurrentSession != null && CurrentSession.Expired())
			{
				_debugNotification?.Log($"Loaded session has expired");
				DestroySession();
				return null;
			}

			// If we aren't online, we can't refresh the token
			if (!Online)
			{
				throw new GotrueException("Only supported when online", Offline);
			}

			// We have a session, and hasn't expired, and we seem to be online. Let's try to refresh it.
			if (Options.AutoRefreshToken && CurrentSession?.RefreshToken != null)
			{
				try
				{
					await RefreshToken();
					return CurrentSession;
				}
				catch (Exception e)
				{
					_debugNotification?.Log($"Failed to refresh token ({e.Message})", e);
					_debugNotification?.Log(JsonConvert.SerializeObject(CurrentSession, Formatting.Indented));
					DestroySession();
					return null;
				}
			}

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
				NotifyAuthStateChange(SignedIn);
				return CurrentSession;
			}

			return null;
		}

		/// <summary>
		/// Headers sent to the API on every request.
		/// </summary>
		public Func<Dictionary<string, string>>? GetHeaders
		{
			get => _api.GetHeaders;
			set => _api.GetHeaders = value;
		}

		/// <summary>
		/// Add a listener to get errors that occur outside of a typical Exception flow.
		/// In particular, this is used to get errors and messages from the background thread
		/// that automatically manages refreshing the user's token.
		/// </summary>
		/// <param name="listener"></param>
		public void AddDebugListener(Action<string, Exception?> listener)
		{
			_debugNotification ??= new DebugNotification();
			_debugNotification.AddDebugListener(listener);
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
				NotifyAuthStateChange(SignedOut);
				return;
			}

			var dirty = CurrentSession != session;
			CurrentSession = session;
			if (dirty) NotifyAuthStateChange(UserUpdated);
		}

		/// <summary>
		/// Clears the session
		/// </summary>
		private void DestroySession()
		{
			UpdateSession(null);
		}

		/// <summary>
		/// Refreshes a Token using the provided token.
		/// </summary>
		/// <returns></returns>
		public async Task RefreshToken(string refreshToken)
		{
			if (string.IsNullOrEmpty(refreshToken))
				throw new GotrueException("No token provided", NoSessionFound);

			var result = await _api.RefreshAccessToken(refreshToken);

			if (result == null || string.IsNullOrEmpty(result.AccessToken))
				throw new GotrueException("Could not refresh token from provided session.", NoSessionFound);

			CurrentSession = result;
			NotifyAuthStateChange(TokenRefreshed);
		}

		/// <summary>
		/// Refreshes a Token. If no token is provided, the current session is used.
		/// </summary>
		/// <returns></returns>
		public async Task RefreshToken()
		{
			if (!Online)
				throw new GotrueException("Only supported when online", Offline);

			if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession?.RefreshToken))
				throw new GotrueException("No current session.", NoSessionFound);

			if (CurrentSession!.Expired())
				throw new GotrueException("Session expired", ExpiredRefreshToken);

			var result = await _api.RefreshAccessToken(CurrentSession.RefreshToken!);

			if (result == null || string.IsNullOrEmpty(result.AccessToken))
				throw new GotrueException("Could not refresh token from provided session.", NoSessionFound);

			CurrentSession = result;

			NotifyAuthStateChange(TokenRefreshed);
		}

		/// <summary>
		/// Loads the session from the persistence provider
		/// </summary>
		public void LoadSession()
		{
			if (_sessionPersistence != null) UpdateSession(_sessionPersistence.Persistence.LoadSession());
		}

		/// <summary>
		/// Retrieves the settings from the server
		/// </summary>
		/// <returns></returns>
		public Task<Settings?> Settings()
		{
			// if(!Online)
			// 	throw new GotrueException("Cannot retrieve settings while offline.", NoSessionFound);
			return _api.Settings();
		}

		/// <summary>
		/// Posts messages and exceptions to the debug listener. This is particularly useful for sorting
		/// out issues with the refresh token background thread.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="e"></param>
		public void Debug(string message, Exception? e = null)
		{
			_debugNotification?.Log(message, e);
		}
	}
}
