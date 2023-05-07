using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using static Supabase.Gotrue.StatelessClient;
using static Supabase.Gotrue.Constants;

namespace GotrueTests
{
	[TestClass]
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
	public class StatelessClientTests
	{
		private const string PASSWORD = "I@M@SuperP@ssWord";

		private static readonly Random Random = new Random();

		private static string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		private static string GetRandomPhoneNumber()
		{
			const string chars = "123456789";
			var inner = new string(Enumerable.Repeat(chars, 10).Select(s => s[Random.Next(s.Length)]).ToArray());

			return $"+1{inner}";
		}

		private string GenerateServiceRoleToken()
		{
			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("37c304f8-51aa-419a-a1af-06154e63707a")); // using GOTRUE_JWT_SECRET

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				IssuedAt = DateTime.Now,
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
				Claims = new Dictionary<string, object>()
				{
					{
						"role", "service_role"
					}
				}
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var securityToken = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(securityToken);
		}

		private StatelessClient _client;


		[TestInitialize]
		public void TestInitializer()
		{
			_client = new StatelessClient();
		}

		private static StatelessClientOptions Options { get => new StatelessClientOptions() { AllowUnconfirmedUserSessions = true }; }

		[TestMethod("StatelessClient: Settings")]
		public async Task Settings()
		{
			var settings = await _client.Settings(Options);
			Assert.IsNotNull(settings);
			Assert.IsFalse(settings.Services["zoom"]);
			Assert.IsTrue(settings.Services["email"]);
			Assert.IsFalse(settings.DisableSignup);
			Assert.IsTrue(settings.MailerAutoConfirm);
			Assert.IsTrue(settings.PhoneAutoConfirm);
			Assert.IsNotNull(settings.SmsProvider);
		}

		[TestMethod("StatelessClient: Signs Up User")]
		public async Task SignsUpUser()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(email, PASSWORD, Options);

			Assert.IsNotNull(session.AccessToken);
			Assert.IsNotNull(session.RefreshToken);
			Assert.IsInstanceOfType(session.User, typeof(User));


			var phone1 = GetRandomPhoneNumber();
			session = await _client.SignUp(SignUpType.Phone, phone1, PASSWORD, Options, new SignUpOptions { Data = new Dictionary<string, object> { { "firstName", "Testing" } } });

			Assert.IsNotNull(session.AccessToken);
			Assert.AreEqual("Testing", session.User.UserMetadata["firstName"]);
		}

		[TestMethod("StatelessClient: Signs Up the same user twice should throw BadRequest")]
		public async Task SignsUpUserTwiceShouldThrowBadRequest()
		{
			var email = $"{RandomString(12)}@supabase.io";
			var result1 = await _client.SignUp(email, PASSWORD, Options);
			Assert.IsNotNull(result1);

			await Assert.ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignUp(email, PASSWORD, Options);
			});
		}

		[TestMethod("StatelessClient: Signs In User (Email, Phone, Refresh token)")]
		public async Task SignsIn()
		{
			// Emails
			var email = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(email, PASSWORD, Options);

			var session = await _client.SignIn(email, PASSWORD, Options);

			Assert.IsNotNull(session.AccessToken);
			Assert.IsNotNull(session.RefreshToken);
			Assert.IsInstanceOfType(session.User, typeof(User));

			// Phones
			var phone = GetRandomPhoneNumber();
			await _client.SignUp(SignUpType.Phone, phone, PASSWORD, Options);


			session = await _client.SignIn(SignInType.Phone, phone, PASSWORD, Options);

			Assert.IsNotNull(session.AccessToken);
			Assert.IsNotNull(session.RefreshToken);
			Assert.IsInstanceOfType(session.User, typeof(User));

			// Refresh Token
			var refreshToken = session.RefreshToken;

			var newSession = await _client.SignIn(SignInType.RefreshToken, refreshToken, options: Options);

			Assert.IsNotNull(newSession.AccessToken);
			Assert.IsNotNull(newSession.RefreshToken);
			Assert.IsInstanceOfType(newSession.User, typeof(User));
		}

		[TestMethod("StatelessClient: Signs Out User (Email)")]
		public async Task SignsOut()
		{
			// Emails
			var email = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(email, PASSWORD, Options);

			var session = await _client.SignIn(email, PASSWORD, Options);

			Assert.IsNotNull(session.AccessToken);
			Assert.IsInstanceOfType(session.User, typeof(User));

			var result = await _client.SignOut(session.AccessToken, Options);

			Assert.IsTrue(result);
		}

		[TestMethod("StatelessClient: Sends Magic Login Email")]
		public async Task SendsMagicLoginEmail()
		{
			var user1 = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user1, PASSWORD, Options);

			var result1 = await _client.SignIn(user1, Options);
			Assert.IsTrue(result1);

			var user2 = $"{RandomString(12)}@supabase.io";
			var result2 = await _client.SignIn(user2, Options, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });
			Assert.IsTrue(result2);
		}

		[TestMethod("StatelessClient: Sends Magic Login Email (Alias)")]
		public async Task SendsMagicLoginEmailAlias()
		{
			var user1 = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user1, PASSWORD, Options);

			var result1 = await _client.SignIn(user1, Options);
			Assert.IsTrue(result1);

			var user2 = $"{RandomString(12)}@supabase.io";
			var result2 = await _client.SignIn(user2, Options, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });
			Assert.IsTrue(result2);
		}

		[TestMethod("StatelessClient: Returns Auth Url for Provider")]
		public void ReturnsAuthUrlForProvider()
		{
			var result1 = _client.SignIn(Provider.Google, Options);
			Assert.AreEqual("http://localhost:9999/authorize?provider=google", result1.Uri.ToString());

			var result2 = _client.SignIn(Provider.Google, Options, new SignInOptions { Scopes = "special scopes please" });
			Assert.AreEqual("http://localhost:9999/authorize?provider=google&scopes=special+scopes+please", result2.Uri.ToString());
		}

		[TestMethod("StatelessClient: Update user")]
		public async Task UpdateUser()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(user, PASSWORD, Options);

			var attributes = new UserAttributes
			{
				Data = new Dictionary<string, object>
				{
					{ "hello", "world" }
				}
			};
			var updateResult = await _client.Update(session.AccessToken, attributes, Options);
			Assert.AreEqual(user, updateResult.Email);
			Assert.IsNotNull(updateResult.UserMetadata);
		}

		[TestMethod("StatelessClient: Returns current user")]
		public async Task GetUser()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(user, PASSWORD, Options);

			Assert.AreEqual(user, session.User.Email);
		}

		[TestMethod("StatelessClient: Throws Exception on Invalid Username and Password")]
		public async Task SignsInUserWrongPassword()
		{
			var user = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(user, PASSWORD, Options);

			await Assert.ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await _client.SignIn(user, PASSWORD + "$", Options);
			});
		}

		[TestMethod("StatelessClient: Sends Invite Email")]
		public async Task SendsInviteEmail()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var serviceRoleKey = GenerateServiceRoleToken();
			var result = await _client.InviteUserByEmail(user, serviceRoleKey, Options);
			Assert.IsTrue(result);
		}

		[TestMethod("StatelessClient: Deletes User")]
		public async Task DeletesUser()
		{
			var user = $"{RandomString(12)}@supabase.io";
			var session = await _client.SignUp(user, PASSWORD, Options);
			var uid = session.User.Id;

			var serviceRoleKey = GenerateServiceRoleToken();
			var result = await _client.DeleteUser(uid, serviceRoleKey, Options);

			Assert.IsTrue(result);
		}

		[TestMethod("StatelessClient: Sends Reset Password Email")]
		public async Task ClientSendsResetPasswordForEmail()
		{
			var email = $"{RandomString(12)}@supabase.io";
			await _client.SignUp(email, PASSWORD, Options);
			var result = await _client.ResetPasswordForEmail(email, Options);
			Assert.IsTrue(result);
		}

		[TestMethod("Client: Lists users")]
		public async Task ClientListUsers()
		{
			var serviceRoleKey = GenerateServiceRoleToken();
			var result = await _client.ListUsers(serviceRoleKey, Options);

			Assert.IsTrue(result.Users.Count > 0);
		}

		[TestMethod("Client: Lists users pagination")]
		public async Task ClientListUsersPagination()
		{
			var serviceRoleKey = GenerateServiceRoleToken();

			var page1 = await _client.ListUsers(serviceRoleKey, Options, page: 1, perPage: 1);
			var page2 = await _client.ListUsers(serviceRoleKey, Options, page: 2, perPage: 1);

			Assert.AreEqual(page1.Users.Count, 1);
			Assert.AreEqual(page2.Users.Count, 1);
			Assert.AreNotEqual(page1.Users[0].Id, page2.Users[0].Id);
		}

		[TestMethod("Client: Lists users sort")]
		public async Task ClientListUsersSort()
		{
			var serviceRoleKey = GenerateServiceRoleToken();

			var result1 = await _client.ListUsers(serviceRoleKey, Options, sortBy: "created_at", sortOrder: SortOrder.Descending);
			var result2 = await _client.ListUsers(serviceRoleKey, Options, sortBy: "created_at", sortOrder: SortOrder.Ascending);

			Assert.AreNotEqual(result1.Users[0].Id, result2.Users[0].Id);
		}

		[TestMethod("Client: Lists users filter")]
		public async Task ClientListUsersFilter()
		{
			var serviceRoleKey = GenerateServiceRoleToken();

			// https://example.com/ is assigned for use for docs & testing
			var result1 = await _client.ListUsers(serviceRoleKey, Options, filter: "@example.com");
			var result2 = await _client.ListUsers(serviceRoleKey, Options, filter: "@supabase.io");

			Assert.AreNotEqual(result2.Users.Count, 0);
			Assert.AreEqual(result1.Users.Count, 0);
			Assert.AreNotEqual(result1.Users.Count, result2.Users.Count);
		}

		[TestMethod("Client: Get User by Id")]
		public async Task ClientGetUserById()
		{
			var serviceRoleKey = GenerateServiceRoleToken();
			var result = await _client.ListUsers(serviceRoleKey, Options, page: 1, perPage: 1);

			var userResult = result.Users[0];
			var userByIdResult = await _client.GetUserById(serviceRoleKey, Options, userResult.Id);

			Assert.AreEqual(userResult.Id, userByIdResult.Id);
			Assert.AreEqual(userResult.Email, userByIdResult.Email);
		}

		[TestMethod("Client: Create a user")]
		public async Task ClientCreateUser()
		{
			var serviceRoleKey = GenerateServiceRoleToken();
			var result = await _client.CreateUser(serviceRoleKey, Options, $"{RandomString(12)}@supabase.io", PASSWORD);

			Assert.IsNotNull(result);

			var attributes = new AdminUserAttributes
			{
				UserMetadata = new Dictionary<string, object> { { "firstName", "123" } },
			};

			var result2 = await _client.CreateUser(serviceRoleKey, Options, $"{RandomString(12)}@supabase.io", PASSWORD, attributes);
			Assert.AreEqual("123", result2.UserMetadata["firstName"]);

			var result3 = await _client.CreateUser(serviceRoleKey, Options, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io", Password = PASSWORD });
			Assert.IsNotNull(result3);
		}

		[TestMethod("Client: Update User by Id")]
		public async Task ClientUpdateUserById()
		{
			var serviceRoleKey = GenerateServiceRoleToken();
			var createdUser = await _client.CreateUser(serviceRoleKey, Options, $"{RandomString(12)}@supabase.io", PASSWORD);

			Assert.IsNotNull(createdUser);

			var updatedUser = await _client.UpdateUserById(serviceRoleKey, Options, createdUser.Id, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io" });

			Assert.IsNotNull(updatedUser);

			Assert.AreEqual(createdUser.Id, updatedUser.Id);
			Assert.AreNotEqual(createdUser.Email, updatedUser.Email);
		}
	}
}
