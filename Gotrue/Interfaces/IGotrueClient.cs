using System;
using System.Threading.Tasks;
using Supabase.Core.Interfaces;
using static Supabase.Gotrue.Constants;
#pragma warning disable CS1591

namespace Supabase.Gotrue.Interfaces
{
	/// <summary>
	/// Interface for the Gotrue Client (auth).
	///
	/// For more information check out the <see cref="Client"/> implementation.
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	/// <typeparam name="TSession"></typeparam>
	public interface IGotrueClient<TUser, TSession> : IGettableHeaders
		where TUser : User
		where TSession : Session
	{
		/// <summary>
		/// Indicates if the client should be considered online or offline.
		/// 
		/// In a server environment, this client would likely always be online.
		///
		/// On a mobile client, you will want to pair this with a network implementation
		/// to turn this on and off as the device goes online and offline.
		/// </summary>
		bool Online { get; set; }

		/// <summary>
		/// The current in-memory session.
		/// </summary>
		TSession? CurrentSession { get; }

		/// <summary>
		/// The current in-memory user.
		/// </summary>
		TUser? CurrentUser { get; }

		/// <summary>
		/// The method that is called when there is a user state change.
		/// </summary>
		delegate void AuthEventHandler(IGotrueClient<TUser, TSession> sender, AuthState stateChanged);

		/// <summary>
		/// Sets the persistence implementation for the client (e.g. file system, local storage, etc).
		/// </summary>
		/// <param name="persistence"></param>
		void SetPersistence(IGotrueSessionPersistence<TSession> persistence);

		/// <summary>
		/// Adds a listener for the user state change event.
		/// </summary>
		/// <param name="authEventHandler"></param>
		void AddStateChangedListener(AuthEventHandler authEventHandler);
		/// <summary>
		/// Removes a listener for the user state change event.
		/// </summary>
		/// <param name="authEventHandler"></param>
		void RemoveStateChangedListener(AuthEventHandler authEventHandler);
		/// <summary>
		/// Removes all listeners for the user state change event - including the persistence listener.
		/// </summary>
		void ClearStateChangedListeners();
		/// <summary>
		/// Notifies all listeners of a user state change. This is called internally by the client.
		/// </summary>
		/// <param name="stateChanged"></param>
		void NotifyAuthStateChange(AuthState stateChanged);

		
		/// <summary>
		/// Converts a URL to a session. For client apps, this probably requires setting up URL handlers.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="storeSession"></param>
		/// <returns></returns>
		Task<TSession?> GetSessionFromUrl(Uri uri, bool storeSession = true);

	
	
		Task<TSession?> RefreshSession();
		Task<bool> ResetPasswordForEmail(string email);
		Task<TSession?> RetrieveSessionAsync();
		Task<bool> SendMagicLink(string email, SignInOptions? options = null);
		TSession SetAuth(string accessToken);
		Task<TSession?> SignIn(SignInType type, string identifierOrToken, string? password = null, string? scopes = null);
		Task<bool> SignIn(string email, SignInOptions? options = null);
		Task<TSession?> SignIn(string email, string password);
		Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessEmailOptions options);
		Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessPhoneOptions options);
		Task<TSession?> SignInWithPassword(string email, string password);
		Task<ProviderAuthState> SignIn(Provider provider, SignInOptions? options = null);
		Task<TSession?> SignInWithIdToken(Provider provider, string idToken, string? nonce = null, string? captchaToken = null);
		Task<TSession?> ExchangeCodeForSession(string codeVerifier, string authCode);
		Task<TSession?> SignUp(SignUpType type, string identifier, string password, SignUpOptions? options = null);
		Task<TSession?> SignUp(string email, string password, SignUpOptions? options = null);
		Task<bool> Reauthenticate();
		Task SignOut();
		Task<TUser?> Update(UserAttributes attributes);
		Task<TSession?> VerifyOTP(string phone, string token, MobileOtpType type = MobileOtpType.SMS);
		Task<TSession?> VerifyOTP(string email, string token, EmailOtpType type = EmailOtpType.MagicLink);
		/// <summary>
		/// Adds a listener for debug messages (e.g. background threads).
		/// </summary>
		/// <param name="logDebug"></param>
		void AddDebugListener(Action<string, Exception?> logDebug);
		/// <summary>
		/// Loads the session from the persistence layer.
		/// </summary>
		void LoadSession();
		/// <summary>
		/// Fetch the server options.
		/// </summary>
		/// <returns></returns>
		Task<Settings?> Settings();

		/// <summary>
		/// Returns the client options.
		/// </summary>
		ClientOptions Options { get; }
		
		/// <summary>
		/// Gets a user from the JWT.
		/// </summary>
		/// <param name="jwt"></param>
		/// <returns></returns>
		Task<TUser?> GetUser(string jwt);

	}
}
