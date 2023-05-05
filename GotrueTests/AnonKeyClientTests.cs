using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static GotrueTests.TestUtils;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
using static Supabase.Gotrue.Constants.AuthState;

namespace GotrueTests
{
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	[TestClass]
	public class AnonKeyClientTests
	{

		private void AuthStateListener(IGotrueClient<User, Session> sender, Constants.AuthState newState)
		{
			if (_stateChanges.Contains(newState) && newState != SignedOut)
				throw new ArgumentException($"State updated twice {newState}");

			_stateChanges.Add(newState);
		}

		private bool AuthStateIsEmpty()
		{
			return _stateChanges.Count == 0;
		}

		[TestInitialize]
		public void TestInitializer()
		{
			_client = new Client(new ClientOptions { AllowUnconfirmedUserSessions = true, PersistSession = true, SessionPersistor = SaveSession, SessionRetriever = LoadSession, SessionDestroyer = DestroySession });
			_client.AddDebugListener(LogDebug);
			_client.AddStateChangedListener(AuthStateListener);
		}

		private void DestroySession()
		{
			_savedSession = null;
		}

		private Session LoadSession()
		{
			return _savedSession;
		}

		private bool SaveSession(Session session)
		{
			_savedSession = session;
			return true;
		}

		private Client _client;

		private readonly List<Constants.AuthState> _stateChanges = new List<Constants.AuthState>();
		private Session _savedSession;

		[TestMethod("Client: Sign Up User")]
		public async Task SignUpUserEmail()
		{
			IsTrue(AuthStateIsEmpty());

			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD);

			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);

			IsNotNull(session.AccessToken);
			IsNotNull(session.RefreshToken);
			IsNotNull(session.User);
		}

		[TestMethod("Client: Sign up Phone")]
		public async Task SignUpUserPhone()
		{
			IsTrue(AuthStateIsEmpty());

			var phone1 = GetRandomPhoneNumber();
			var session = await _client.SignUp(Constants.SignUpType.Phone, phone1, PASSWORD, new SignUpOptions { Data = new Dictionary<string, object> { { "firstName", "Testing" } } });

			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);

			IsNotNull(session.AccessToken);
			AreEqual("Testing", session.User.UserMetadata["firstName"]);
		}

		[TestMethod("Client: Triggers Token Refreshed Event")]
		public async Task ClientTriggersTokenRefreshedEvent()
		{
			var tsc = new TaskCompletionSource<string>();

			var email = $"{RandomString(12)}@supabase.io";

			IsTrue(AuthStateIsEmpty());

			var user = await _client.SignUp(email, PASSWORD);

			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);

			_client.AddStateChangedListener((_, args) =>
			{
				if (args == TokenRefreshed)
				{
					tsc.SetResult(_client.CurrentSession.AccessToken);
				}
			});

			_stateChanges.Clear();

			await _client.RefreshSession();
			Contains(_stateChanges, TokenRefreshed);
			AreEqual(_client.CurrentSession, _savedSession);

			var newToken = await tsc.Task;
			IsNotNull(newToken);
			AreNotEqual(user.RefreshToken, _client.CurrentSession.RefreshToken);
		}

		[TestMethod("Client: Signs In User (Email, Phone, Refresh token)")]
		public async Task ClientSignsIn()
		{
			var email = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(email, PASSWORD);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			var session = await _client.SendMagicLinkEmail(email, PASSWORD);

			IsNotNull(session.AccessToken);
			IsNotNull(session.RefreshToken);
			IsNotNull(session.User);

			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			// Phones
			var phone = GetRandomPhoneNumber();
			await _client.SignUp(Constants.SignUpType.Phone, phone, PASSWORD);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			IsNull(_savedSession);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			session = await _client.SendMagicLinkEmail(Constants.SignInType.Phone, phone, PASSWORD);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			IsNotNull(session.AccessToken);
			IsNotNull(session.RefreshToken);
			IsNotNull(session.User);

			// Refresh Token
			var refreshToken = session.RefreshToken;

			var newSession = await _client.SendMagicLinkEmail(Constants.SignInType.RefreshToken, refreshToken);
			AreEqual(_client.CurrentSession, _savedSession);
			Contains(_stateChanges, TokenRefreshed);
			DoesNotContain(_stateChanges, SignedIn);

			IsNotNull(newSession.AccessToken);
			IsNotNull(newSession.RefreshToken);
			IsNotNull(newSession.User);
		}

		[TestMethod("Client: Sends Magic Login Email")]
		public async Task ClientSendsMagicLoginEmail()
		{
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			var result = await _client.SendMagicLinkEmail(user);
			IsTrue(result);
			AreEqual(0, _stateChanges.Count);
			AreEqual(_client.CurrentSession, _savedSession);
		}

		[TestMethod("Client: Sends Magic Login Email (Alias)")]
		public async Task ClientSendsMagicLoginEmailAlias()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var user2 = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			IsNull(_savedSession);
			AreEqual(_client.CurrentSession, _savedSession);

			var result = await _client.SendMagicLink(user);
			var result2 = await _client.SendMagicLink(user2, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });

			IsTrue(result);
			IsTrue(result2);
		}

		[TestMethod("Client: Returns Auth Url for Provider")]
		public async Task ClientReturnsAuthUrlForProvider()
		{
			var result1 = await _client.SendMagicLinkEmail(Constants.Provider.Google);
			AreEqual("http://localhost:9999/authorize?provider=google", result1.Uri.ToString());

			var result2 = await _client.SendMagicLinkEmail(Constants.Provider.Google, new SignInOptions { Scopes = "special scopes please" });
			AreEqual("http://localhost:9999/authorize?provider=google&scopes=special+scopes+please", result2.Uri.ToString());
		}

		[TestMethod("Client: Returns Verification Code for Provider")]
		public async Task ClientReturnsPKCEVerifier()
		{
			var result = await _client.SendMagicLinkEmail(Constants.Provider.Github, new SignInOptions { FlowType = Constants.OAuthFlowType.PKCE });

			Contains(_stateChanges, SignedOut);
			IsNull(_savedSession);

			IsTrue(!string.IsNullOrEmpty(result.PKCEVerifier));
			IsTrue(result.Uri.Query.Contains("flow_type=pkce"));
			IsTrue(result.Uri.Query.Contains("code_challenge="));
			IsTrue(result.Uri.Query.Contains("code_challenge_method=s256"));
			IsTrue(result.Uri.Query.Contains("provider=github"));
		}

		[TestMethod("Client: Update user")]
		public async Task ClientUpdateUser()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD);
			IsNotNull(session);
			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			var attributes = new UserAttributes { Data = new Dictionary<string, object> { { "hello", "world" } } };
			var result = await _client.Update(attributes);
			IsNotNull(result);
			AreEqual(email, _client.CurrentUser.Email);
			IsNotNull(_client.CurrentUser.UserMetadata);
			Contains(_stateChanges, UserUpdated);
			AreEqual(_client.CurrentSession, _savedSession);

			await _client.SignOut();

		}

		[TestMethod("Client: Returns current user")]
		public async Task ClientGetUser()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var newUser = await _client.SignUp(email, PASSWORD);

			AreEqual(email, _client.CurrentUser.Email);

			var userByJWT = await _client.GetUser(newUser.AccessToken ?? throw new InvalidOperationException());
			AreEqual(email, userByJWT.Email);
		}

		[TestMethod("Client: Nulls CurrentUser on SignOut")]
		public async Task ClientGetUserAfterLogOut()
		{
			IsTrue(AuthStateIsEmpty());
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			Contains(_stateChanges, SignedIn);

			_stateChanges.Clear();
			await _client.SignOut();

			Contains(_stateChanges, SignedOut);

			IsNull(_client.CurrentUser);
		}

		[TestMethod("Client: Throws Exception on Invalid Username and Password")]
		public async Task ClientSignsInUserWrongPassword()
		{
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);

			await _client.SignOut();

			await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				var result = await _client.SendMagicLinkEmail(user, PASSWORD + "$");
				IsNotNull(result);
			});
		}


		[TestMethod("Client: Send Reset Password Email")]
		public async Task ClientSendsResetPasswordForEmail()
		{
			var email = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(email, PASSWORD);
			var result = await _client.ResetPasswordForEmail(email);
			IsTrue(result);
		}

	}
}
