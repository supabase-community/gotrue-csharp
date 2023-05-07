using System;
using System.Threading.Tasks;
using Supabase.Core.Interfaces;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue.Interfaces
{
	public interface IGotrueClient<TUser, TSession> : IGettableHeaders
		where TUser : User
		where TSession : Session
	{
		TSession? CurrentSession { get; }
		TUser? CurrentUser { get; }

		delegate void AuthEventHandler(IGotrueClient<TUser, TSession> sender, AuthState stateChanged);

		public void SetPersistence(IGotrueSessionPersistence<TSession> persistence);

		public void AddStateChangedListener(AuthEventHandler authEventHandler);
		public void RemoveStateChangedListener(AuthEventHandler authEventHandler);
		public void ClearStateChangedListeners();
		public void NotifyAuthStateChange(AuthState stateChanged);

		Task<TUser?> CreateUser(string jwt, AdminUserAttributes attributes);
		Task<TUser?> CreateUser(string jwt, string email, string password, AdminUserAttributes? attributes = null);
		Task<bool> DeleteUser(string uid, string jwt);
		Task<TSession?> GetSessionFromUrl(Uri uri, bool storeSession = true);
		Task<TUser?> GetUser(string jwt);
		Task<TUser?> GetUserById(string jwt, string userId);
		Task<bool> InviteUserByEmail(string email, string jwt);
		Task<UserList<TUser>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null, int? perPage = null);
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
		Task SignOut();
		Task<TUser?> Update(UserAttributes attributes);
		Task<TUser?> UpdateUserById(string jwt, string userId, AdminUserAttributes userData);
		Task<TSession?> VerifyOTP(string phone, string token, MobileOtpType type = MobileOtpType.SMS);
		Task<TSession?> VerifyOTP(string email, string token, EmailOtpType type = EmailOtpType.MagicLink);
	}
}