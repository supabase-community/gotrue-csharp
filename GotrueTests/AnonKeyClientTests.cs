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
			_client = new Client(new ClientOptions<Session> { AllowUnconfirmedUserSessions = true });
			_client.AddDebugListener(LogDebug);
			_client.AddStateChangedListener(AuthStateListener);
		}

		private Client _client;

		private readonly List<Constants.AuthState> _stateChanges = new List<Constants.AuthState>();

		[TestMethod("Client: Sign Up User")]
		public async Task SignUpUserEmail()
		{
			IsTrue(AuthStateIsEmpty());

			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD);

			Contains(_stateChanges, SignedIn);

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

			IsNotNull(session.AccessToken);
			AreEqual("Testing", session.User.UserMetadata["firstName"]);
		}

		[TestMethod("Client: Signs Up the same user twice should throw BadRequestException")]
		public async Task SignsUpUserTwiceShouldReturnBadRequest()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var result1 = await _client.SignUp(email, PASSWORD);

			IsNotNull(result1);

			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				// This calls session destroy, logging the user out
				await _client.SignUp(email, PASSWORD);
			});

			Contains(_stateChanges, SignedOut);
		}

		[TestMethod("Client: Triggers Token Refreshed Event")]
		public async Task ClientTriggersTokenRefreshedEvent()
		{
			var tsc = new TaskCompletionSource<string>();

			var email = $"{RandomString(12)}@supabase.io";

			IsTrue(AuthStateIsEmpty());

			var user = await _client.SignUp(email, PASSWORD);

			Contains(_stateChanges, SignedIn);

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
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			_stateChanges.Clear();

			var session = await _client.SignIn(email, PASSWORD);

			IsNotNull(session.AccessToken);
			IsNotNull(session.RefreshToken);
			IsNotNull(session.User);

			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			// Phones
			var phone = GetRandomPhoneNumber();
			await _client.SignUp(Constants.SignUpType.Phone, phone, PASSWORD);
			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			_stateChanges.Clear();

			session = await _client.SignIn(Constants.SignInType.Phone, phone, PASSWORD);
			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			IsNotNull(session.AccessToken);
			IsNotNull(session.RefreshToken);
			IsNotNull(session.User);

			// Refresh Token
			var refreshToken = session.RefreshToken;

			var newSession = await _client.SignIn(Constants.SignInType.RefreshToken, refreshToken);
			Contains(_stateChanges, TokenRefreshed);
			DoesNotContain(_stateChanges, SignedIn);

			IsNotNull(newSession.AccessToken);
			IsNotNull(newSession.RefreshToken);
			IsInstanceOfType(newSession.User, typeof(User));
		}

		[TestMethod("Client: Sends Magic Login Email")]
		public async Task ClientSendsMagicLoginEmail()
		{
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);
			_stateChanges.Clear();

			var result = await _client.SignIn(user);
			IsTrue(result);
			Contains(_stateChanges, SignedOut);
		}

		[TestMethod("Client: Sends Magic Login Email (Alias)")]
		public async Task ClientSendsMagicLoginEmailAlias()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var user2 = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			Contains(_stateChanges, SignedIn);
			_stateChanges.Clear();

			await _client.SignOut();
			Contains(_stateChanges, SignedOut);

			var result = await _client.SendMagicLink(user);
			var result2 = await _client.SendMagicLink(user2, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });

			IsTrue(result);
			IsTrue(result2);
		}
		
		[TestMethod("Client: Returns Auth Url for Provider")]
		public async Task ClientReturnsAuthUrlForProvider()
		{
			var result1 = await _client.SignIn(Constants.Provider.Google);
			AreEqual("http://localhost:9999/authorize?provider=google", result1.Uri.ToString());
			
			var result2 = await _client.SignIn(Constants.Provider.Google, new SignInOptions { Scopes = "special scopes please" });
			AreEqual("http://localhost:9999/authorize?provider=google&scopes=special+scopes+please", result2.Uri.ToString());
		}

		[TestMethod("Client: Returns Verification Code for Provider")]
		public async Task ClientReturnsPKCEVerifier()
		{
			var result = await _client.SignIn(Constants.Provider.Github, new SignInOptions { FlowType = Constants.OAuthFlowType.PKCE });

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

			var attributes = new UserAttributes { Data = new Dictionary<string, object> { { "hello", "world" } } };
			var result = await _client.Update(attributes);
			AreEqual(email, _client.CurrentUser.Email);
			IsNotNull(_client.CurrentUser.UserMetadata);

			await _client.SignOut();
			var token = GenerateServiceRoleToken();
			var result2 = await _client.UpdateUserById(token, session.User.Id ?? throw new InvalidOperationException(), new AdminUserAttributes { UserMetadata = new Dictionary<string, object> { { "hello", "updated" } } });

			AreNotEqual(result.UserMetadata["hello"], result2.UserMetadata["hello"]);
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
				var result = await _client.SignIn(user, PASSWORD + "$");
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
