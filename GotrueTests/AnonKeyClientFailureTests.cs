using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static GotrueTests.TestUtils;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
using static Supabase.Gotrue.Constants.AuthState;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace GotrueTests
{
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	[TestClass]
	public class AnonKeyClientFailureTests
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

		[TestMethod("Client: Sign Up With Bad Password")]
		public async Task SignUpUserEmailBadPassword()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignUp(email, "x");
			});
			AreEqual(BadPassword, x.Reason);
			IsNull(_savedSession);
			Contains(_stateChanges, SignedOut);
			AreEqual(1, _stateChanges.Count);
		}

		[TestMethod("Client: Sign Up With Bad Email Address")]
		public async Task SignUpUserEmailBadEmailAddress()
		{
			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignUp("not a real email address", PASSWORD);
			});
			AreEqual(BadEmailAddress, x.Reason);
			IsNull(_savedSession);
			Contains(_stateChanges, SignedOut);
			AreEqual(1, _stateChanges.Count);
		}

		[TestMethod("Client: Sign up without a phone number")]
		public async Task SignUpUserPhone()
		{
			IsTrue(AuthStateIsEmpty());

			var phone1 = "";
			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignUp(Constants.SignUpType.Phone, phone1, PASSWORD, new SignUpOptions { Data = new Dictionary<string, object> { { "firstName", "Testing" } } });
			});
			AreEqual(MissingInformation, x.Reason);
			IsNull(_savedSession);
			Contains(_stateChanges, SignedOut);
			AreEqual(1, _stateChanges.Count);
		}

		[TestMethod("Client: Signs Up the same user twice")]
		public async Task SignsUpUserTwiceShouldReturnBadRequest()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD);

			IsNotNull(session);

			Contains(_stateChanges, SignedIn);
			AreEqual(_client.CurrentSession, _savedSession);
			_stateChanges.Clear();

			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignUp(email, PASSWORD);
			});

			AreEqual(AlreadyRegistered, x.Reason);
			IsNull(_savedSession);
			Contains(_stateChanges, SignedOut);
			AreEqual(1, _stateChanges.Count);
		}

		[TestMethod("Client: Bogus refresh token")]
		public async Task ClientTriggersTokenRefreshedEvent()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var user = await _client.SignUp(email, PASSWORD);
			IsNotNull(user);

			_client.CurrentSession.RefreshToken = "bogus token";

			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.RefreshSession();
			});
			AreEqual(x.Reason, InvalidRefreshToken);
		}

		[TestMethod("Client: Send Reset Password Email for unknown email")]
		public async Task ClientSendsResetPasswordForEmail()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var result = await _client.ResetPasswordForEmail(email);
			IsTrue(result);
		}
	}
}