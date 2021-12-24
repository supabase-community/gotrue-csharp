using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.Client;

namespace GotrueTests
{
    [TestClass]
    public class Client
    {
        private Supabase.Gotrue.Client client;

        private string password = "I@M@SuperP@ssWord";

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GetRandomPhoneNumber()
        {
            const string chars = "123456789";
            var inner = new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"+1{inner}";
        }

        private string GenerateServiceRoleToken()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("37c304f8-51aa-419a-a1af-06154e63707a")); // using GOTRUE_JWT_SECRET

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                IssuedAt = DateTime.Now,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials =
                    new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
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

        [TestInitialize]
        public async Task TestInitializer()
        {
            client = await InitializeAsync();
        }

        [TestMethod("Client: Signs Up User")]
        public async Task ClientSignsUpUser()
        {
            Session session = null;
            var email = $"{RandomString(12)}@supabase.io";
            session = await client.SignUp(email, password);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));


            var phone1 = GetRandomPhoneNumber();
            session = await client.SignUp(SignUpType.Phone, phone1, password, new Dictionary<string, object> { { "firstName", "Testing" } });

            Assert.IsNotNull(session.AccessToken);
            Assert.AreEqual("Testing", session.User.UserMetadata["firstName"]);
        }

        [TestMethod("Client: Signs Up the same user twice should throw BadRequestException")]
        public async Task ClientSignsUpUserTwiceShouldReturnBadRequest()
        {
            var email = $"{RandomString(12)}@supabase.io";
            var result1 = await client.SignUp(email, password);

            await Assert.ThrowsExceptionAsync<BadRequestException>(async () =>
            {
                await client.SignUp(email, password);
            });
        }

        [TestMethod("Client: Triggers Token Refreshed Event")]
        public async Task ClientTriggersTokenRefreshedEvent()
        {
            var tsc = new TaskCompletionSource<string>();

            var email = $"{RandomString(12)}@supabase.io";
            var user = await client.SignUp(email, password);

            client.StateChanged += (sender, args) =>
            {
                if (args.State == AuthState.TokenRefreshed)
                {
                    tsc.SetResult(client.CurrentSession.AccessToken);
                }
            };

            await client.RefreshSession();

            var newToken = await tsc.Task;

            Assert.AreNotEqual(user.RefreshToken, client.CurrentSession.RefreshToken);
        }

        [TestMethod("Client: Signs In User (Email, Phone, Refresh token)")]
        public async Task ClientSignsIn()
        {
            Session session = null;
            string refreshToken = "";

            // Emails
            var email = $"{RandomString(12)}@supabase.io";
            await client.SignUp(email, password);

            await client.SignOut();

            session = await client.SignIn(email, password);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));

            // Phones
            var phone = GetRandomPhoneNumber();
            await client.SignUp(SignUpType.Phone, phone, password);

            await client.SignOut();

            session = await client.SignIn(SignInType.Phone, phone, password);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));

            // Refresh Token
            refreshToken = session.RefreshToken;

            var newSession = await client.SignIn(SignInType.RefreshToken, refreshToken);

            Assert.IsNotNull(newSession.AccessToken);
            Assert.IsNotNull(newSession.RefreshToken);
            Assert.IsInstanceOfType(newSession.User, typeof(User));
        }

        [TestMethod("Client: Sends Magic Login Email")]
        public async Task ClientSendsMagicLoginEmail()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            await client.SignOut();

            var result = await client.SignIn(user);
            Assert.IsTrue(result);
        }

        [TestMethod("Client: Sends Magic Login Email (Alias)")]
        public async Task ClientSendsMagicLoginEmailAlias()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            await client.SignOut();

            var result = await client.SendMagicLink(user);
            Assert.IsTrue(result);
        }

        [TestMethod("Client: Returns Auth Url for Provider")]
        public async Task ClientReturnsAuthUrlForProvider()
        {
            var result1 = await client.SignIn(Provider.Google);
            Assert.AreEqual("http://localhost:9999/authorize?provider=google", result1);

            var result2 = await client.SignIn(Provider.Google, "special scopes please");
            Assert.AreEqual("http://localhost:9999/authorize?provider=google&scopes=special+scopes+please", result2);
        }

        [TestMethod("Client: Update user")]
        public async Task ClientUpdateUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    {"hello", "world" }
                }
            };
            await client.Update(attributes);
            Assert.AreEqual(user, client.CurrentUser.Email);
            Assert.IsNotNull(client.CurrentUser.UserMetadata);
        }

        [TestMethod("Client: Returns current user")]
        public async Task ClientGetUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            Assert.AreEqual(user, client.CurrentUser.Email);
        }

        [TestMethod("Client: Nulls CurrentUser on SignOut")]
        public async Task ClientGetUserAfterLogOut()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            await client.SignOut();

            Assert.IsNull(client.CurrentUser);
        }

        [TestMethod("Client: Throws Exception on Invalid Username and Password")]
        public async Task ClientSignsInUserWrongPassword()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            await client.SignOut();

            await Assert.ThrowsExceptionAsync<BadRequestException>(async () =>
            {
                var result = await client.SignIn(user, password + "$");
            });

        }

        [TestMethod("Client: Sends Invite Email")]
        public async Task ClientSendsInviteEmail()
        {
            var user = $"{RandomString(12)}@supabase.io";
            var service_role_key = GenerateServiceRoleToken();
            var result = await client.InviteUserByEmail(user, service_role_key);
            Assert.IsTrue(result);
        }

        [TestMethod("Client: Lists users")]
        public async Task ClientListUsers()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await client.ListUsers(service_role_key);

            Assert.IsTrue(result.Users.Count > 0);
        }

        [TestMethod("Client: Lists users pagination")]
        public async Task ClientListUsersPagination()
        {
            var service_role_key = GenerateServiceRoleToken();

            var page1 = await client.ListUsers(service_role_key, page: 1, perPage: 1);
            var page2 = await client.ListUsers(service_role_key, page: 2, perPage: 1);

            Assert.AreEqual(page1.Users.Count, 1);
            Assert.AreEqual(page2.Users.Count, 1);
            Assert.AreNotEqual(page1.Users[0].Id, page2.Users[0].Id);
        }

        [TestMethod("Client: Lists users sort")]
        public async Task ClientListUsersSort()
        {
            var service_role_key = GenerateServiceRoleToken();

            var result1 = await client.ListUsers(service_role_key, sortBy: "created_at", sortOrder: SortOrder.Ascending);
            var result2 = await client.ListUsers(service_role_key, sortBy: "created_at", sortOrder: SortOrder.Descending);

            Assert.AreNotEqual(result1.Users[0].Id, result2.Users[0].Id);
        }

        [TestMethod("Client: Lists users filter")]
        public async Task ClientListUsersFilter()
        {
            var service_role_key = GenerateServiceRoleToken();

            var result1 = await client.ListUsers(service_role_key, filter: "@nonexistingrandomemailprovider.com");
            var result2 = await client.ListUsers(service_role_key, filter: "@supabase.io");

            Assert.AreNotEqual(result2.Users.Count, 0);
            Assert.AreEqual(result1.Users.Count, 0);
            Assert.AreNotEqual(result1.Users.Count, result2.Users.Count);
        }

        [TestMethod("Client: Get User by Id")]
        public async Task ClientGetUserById()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await client.ListUsers(service_role_key, page: 1, perPage: 1);

            var userResult = result.Users[0];
            var userByIdResult = await client.GetUserById(service_role_key, userResult.Id);

            Assert.AreEqual(userResult.Id, userByIdResult.Id);
            Assert.AreEqual(userResult.Email, userByIdResult.Email);
        }

        [TestMethod("Client: Create a user")]
        public async Task ClientCreateUser()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await client.CreateUser(service_role_key, $"{RandomString(12)}@supabase.io", password);

            Assert.IsNotNull(result);


            var attributes = new AdminUserAttributes
            {
                UserMetadata = new Dictionary<string, object> { { "firstName", "123" } },
                AppMetadata = new Dictionary<string, object> { { "roles", new List<string> { "editor", "publisher" } } }
            };

            var result2 = await client.CreateUser(service_role_key, $"{RandomString(12)}@supabase.io", password, attributes);
            Assert.AreEqual("123", result2.UserMetadata["firstName"]);

            var result3 = await client.CreateUser(service_role_key, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io", Password = password });
            Assert.IsNotNull(result3);
        }


        [TestMethod("Client: Update User by Id")]
        public async Task ClientUpdateUserById()
        {
            var service_role_key = GenerateServiceRoleToken();
            var createdUser = await client.CreateUser(service_role_key, $"{RandomString(12)}@supabase.io", password);

            Assert.IsNotNull(createdUser);

            var updatedUser = await client.UpdateUserById(service_role_key, createdUser.Id, new UserAttributes { Email = $"{RandomString(12)}@supabase.io" });

            Assert.IsNotNull(updatedUser);

            Assert.AreEqual(createdUser.Id, updatedUser.Id);
            Assert.AreNotEqual(createdUser.Email, updatedUser.Email);
        }

        [TestMethod("Client: Deletes User")]
        public async Task ClientDeletesUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);
            var uid = client.CurrentUser.Id;

            var service_role_key = GenerateServiceRoleToken();
            var result = await client.DeleteUser(uid, service_role_key);

            Assert.IsTrue(result);
        }

        [TestMethod("Client: Sends Reset Password Email")]
        public async Task ClientSendsResetPasswordForEmail()
        {
            var email = $"{RandomString(12)}@supabase.io";
            await client.SignUp(email, password);
            var result = await client.ResetPasswordForEmail(email);
            Assert.IsTrue(result);
        }


    }
}
