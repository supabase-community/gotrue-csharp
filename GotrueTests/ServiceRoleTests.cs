using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using static Supabase.Gotrue.Constants;
using static GotrueTests.TestUtils;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;

namespace GotrueTests
{
	[TestClass]
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	public class ServiceRoleTests
	{
		private Client _client;

		private readonly string _serviceKey = GenerateServiceRoleToken();

		[TestInitialize]
		public void TestInitializer()
		{
			_client = new Client(new ClientOptions { AllowUnconfirmedUserSessions = true });
			_client.AddDebugListener(LogDebug);
		}

		[TestMethod("Service Role: Update User")]
		public async Task UpdateUser()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD);
			IsNotNull(session);

			var attributes = new UserAttributes { Data = new Dictionary<string, object> { { "hello", "world" } } };
			var result = await _client.Update(attributes);
			IsNotNull(result);
			AreEqual(email, _client.CurrentUser.Email);
			IsNotNull(_client.CurrentUser.UserMetadata);

			await _client.SignOut();

			var result2 = await _client.UpdateUserById(_serviceKey, session.User.Id!, new AdminUserAttributes { UserMetadata = new Dictionary<string, object> { { "hello", "updated" } } });

			AreNotEqual(result.UserMetadata["hello"], result2.UserMetadata["hello"]);
		}

		[TestMethod("Service Role: Send Invite Email")]
		public async Task SendsInviteEmail()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var result = await _client.InviteUserByEmail(user, _serviceKey);
			Assert.IsTrue(result);
		}

		[TestMethod("Service Role: List users")]
		public async Task ListUsers()
		{
			var result = await _client.ListUsers(_serviceKey);
			Assert.IsTrue(result.Users.Count > 0);
		}

		[TestMethod("Service Role: List users by page")]
		public async Task ListUsersPagination()
		{
			var page1 = await _client.ListUsers(_serviceKey, page: 1, perPage: 1);
			var page2 = await _client.ListUsers(_serviceKey, page: 2, perPage: 1);

			Assert.AreEqual(page1.Users.Count, 1);
			Assert.AreEqual(page2.Users.Count, 1);
			Assert.AreNotEqual(page1.Users[0].Id, page2.Users[0].Id);
		}

		[TestMethod("Service Role: Lists users sort")]
		public async Task ListUsersSort()
		{
			var serviceRoleKey = GenerateServiceRoleToken();

			var result1 = await _client.ListUsers(serviceRoleKey, sortBy: "created_at", sortOrder: SortOrder.Ascending);
			var result2 = await _client.ListUsers(serviceRoleKey, sortBy: "created_at", sortOrder: SortOrder.Descending);

			Assert.AreNotEqual(result1.Users[0].Id, result2.Users[0].Id);
		}

		[TestMethod("Service role: Lists users with filter")]
		public async Task ListUsersFilter()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var result = await _client.SignUp(user, PASSWORD);
			Assert.IsNotNull(result);

			// ReSharper disable once StringLiteralTypo
			var result1 = await _client.ListUsers(_serviceKey, filter: "@nonexistingrandomemailprovider.com");
			var result2 = await _client.ListUsers(_serviceKey, filter: "@supabase.io");

			Assert.AreNotEqual(result2.Users.Count, 0);
			Assert.AreEqual(result1.Users.Count, 0);
			Assert.AreNotEqual(result1.Users.Count, result2.Users.Count);
		}

		[TestMethod("Service Role: Get User by Id")]
		public async Task GetUserById()
		{
			var result = await _client.ListUsers(_serviceKey, page: 1, perPage: 1);

			var userResult = result.Users[0];
			var userByIdResult = await _client.GetUserById(_serviceKey, userResult.Id ?? throw new InvalidOperationException());

			Assert.AreEqual(userResult.Id, userByIdResult.Id);
			Assert.AreEqual(userResult.Email, userByIdResult.Email);
		}

		[TestMethod("Service Role: Create a user")]
		public async Task CreateUser()
		{
			var result = await _client.CreateUser(_serviceKey, $"{RandomString(12)}@supabase.io", PASSWORD);

			Assert.IsNotNull(result);

			var attributes = new AdminUserAttributes
			{
				UserMetadata = new Dictionary<string, object> { { "firstName", "123" } },
				AppMetadata = new Dictionary<string, object> { { "roles", new List<string> { "editor", "publisher" } } }
			};

			var result2 = await _client.CreateUser(_serviceKey, $"{RandomString(12)}@supabase.io", PASSWORD, attributes);
			Assert.AreEqual("123", result2.UserMetadata["firstName"]);

			var result3 = await _client.CreateUser(_serviceKey, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io", Password = PASSWORD });
			Assert.IsNotNull(result3);
		}

		[TestMethod("Service Role: Update User by Id")]
		public async Task UpdateUserById()
		{
			var createdUser = await _client.CreateUser(_serviceKey, $"{RandomString(12)}@supabase.io", PASSWORD);

			Assert.IsNotNull(createdUser);

			var updatedUser = await _client.UpdateUserById(_serviceKey, createdUser.Id ?? throw new InvalidOperationException(), new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io" });

			Assert.IsNotNull(updatedUser);

			Assert.AreEqual(createdUser.Id, updatedUser.Id);
			Assert.AreNotEqual(createdUser.Email, updatedUser.Email);
		}

		[TestMethod("Service Role: Delete User")]
		public async Task DeletesUser()
		{
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD);
			var uid = _client.CurrentUser.Id;

			var result = await _client.DeleteUser(uid ?? throw new InvalidOperationException(), _serviceKey);

			Assert.IsTrue(result);
		}

		[TestMethod("Nonce generation and verification")]
		public void NonceGeneration()
		{
			var nonce = Helpers.GenerateNonce();
			Assert.IsNotNull(nonce);
			Assert.AreEqual(128, nonce.Length);

			var pkceVerifier = Helpers.GeneratePKCENonceVerifier(nonce);
			Assert.IsNotNull(pkceVerifier);
			Assert.AreEqual(43, pkceVerifier.Length);

			var appleVerifier = Helpers.GenerateSHA256NonceFromRawNonce(nonce);
			Assert.IsNotNull(appleVerifier);
			Assert.AreEqual(64, appleVerifier.Length);

			const string helloNonce = "hello_world_nonce";

			var helloPkceVerifier = Helpers.GeneratePKCENonceVerifier(helloNonce);
			Assert.IsNotNull(helloPkceVerifier);
			// ReSharper disable once StringLiteralTypo
			Assert.AreEqual("9TMmi4JOlYOQEP2Ha39WXj9pySILGnAfQsz-yXws0yE", helloPkceVerifier);

			var helloAppleVerifier = Helpers.GenerateSHA256NonceFromRawNonce(helloNonce);
			Assert.IsNotNull(helloAppleVerifier);
			Assert.AreEqual("f533268b824e95839010fd876b7f565e3f69c9220b1a701f42ccfec97c2cd321", helloAppleVerifier);
		}
	}
}
