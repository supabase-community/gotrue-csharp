using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using static Supabase.Gotrue.Client;
using static Supabase.Gotrue.StatelessClient;
using static Supabase.Gotrue.Constants;
using Supabase.Gotrue.Interfaces;

namespace GotrueTests
{
    [TestClass]
    public class StatelessClient
    {
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

        StatelessClientOptions options => new StatelessClientOptions() { AllowUnconfirmedUserSessions = true };


        [TestMethod("StatelessClient: Signs Up User")]
        public async Task SignsUpUser()
        {
            var email = $"{RandomString(12)}@supabase.io";
            var session = await SignUp(email, password, options);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));


            var phone1 = GetRandomPhoneNumber();
            session = await SignUp(SignUpType.Phone, phone1, password, options, new SignUpOptions { Data = new Dictionary<string, object> { { "firstName", "Testing" } } });

            Assert.IsNotNull(session.AccessToken);
            Assert.AreEqual("Testing", session.User.UserMetadata["firstName"]);
        }

        [TestMethod("StatelessClient: Signs Up the same user twice should throw BadRequest")]
        public async Task SignsUpUserTwiceShouldThrowBadRequest()
        {
            var email = $"{RandomString(12)}@supabase.io";
            var result1 = await SignUp(email, password, options);

            await Assert.ThrowsExceptionAsync<BadRequestException>(async () =>
            {
                await SignUp(email, password, options);
            });
        }

        [TestMethod("StatelessClient: Signs In User (Email, Phone, Refresh token)")]
        public async Task SignsIn()
        {
            string refreshToken = "";

            // Emails
            var email = $"{RandomString(12)}@supabase.io";
            await SignUp(email, password, options);


            var session = await SignIn(email, password, options);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));

            // Phones
            var phone = GetRandomPhoneNumber();
            await SignUp(SignUpType.Phone, phone, password, options);


            session = await SignIn(SignInType.Phone, phone, password, options);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsNotNull(session.RefreshToken);
            Assert.IsInstanceOfType(session.User, typeof(User));

            // Refresh Token
            refreshToken = session.RefreshToken;

            var newSession = await SignIn(SignInType.RefreshToken, refreshToken, options: options);

            Assert.IsNotNull(newSession.AccessToken);
            Assert.IsNotNull(newSession.RefreshToken);
            Assert.IsInstanceOfType(newSession.User, typeof(User));
        }

        [TestMethod("StatelessClient: Signs Out User (Email)")]
        public async Task SignsOut()
        {
            // Emails
            var email = $"{RandomString(12)}@supabase.io";
            await SignUp(email, password, options);


            var session = await SignIn(email, password, options);

            Assert.IsNotNull(session.AccessToken);
            Assert.IsInstanceOfType(session.User, typeof(User));

            var result = await SignOut(session.AccessToken, options);

            Assert.IsTrue(result);
        }

        [TestMethod("StatelessClient: Sends Magic Login Email")]
        public async Task SendsMagicLoginEmail()
        {
            var user1 = $"{RandomString(12)}@supabase.io";
            await SignUp(user1, password, options);

            var result1 = await SignIn(user1, options);
            Assert.IsTrue(result1);

            var user2 = $"{RandomString(12)}@supabase.io";
            var result2 = await SignIn(user2, options, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });
            Assert.IsTrue(result2);
        }

        [TestMethod("StatelessClient: Sends Magic Login Email (Alias)")]
        public async Task SendsMagicLoginEmailAlias()
        {
            var user1 = $"{RandomString(12)}@supabase.io";
            await SignUp(user1, password, options);

            var result1 = await SignIn(user1, options);
            Assert.IsTrue(result1);

            var user2 = $"{RandomString(12)}@supabase.io";
            var result2 = await SignIn(user2, options, new SignInOptions { RedirectTo = $"com.{RandomString(12)}.deeplink://login" });
            Assert.IsTrue(result2);
        }

        [TestMethod("StatelessClient: Returns Auth Url for Provider")]
        public void ReturnsAuthUrlForProvider()
        {
            var result1 = SignIn(Provider.Google, options);
            Assert.AreEqual("http://localhost:9999/authorize?provider=google", result1);

            var result2 = SignIn(Provider.Google, options, "special scopes please");
            Assert.AreEqual("http://localhost:9999/authorize?provider=google&scopes=special+scopes+please", result2);
        }

        [TestMethod("StatelessClient: Update user")]
        public async Task UpdateUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            var session = await SignUp(user, password, options);

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    {"hello", "world" }
                }
            };
            var updateResult = await Update(session.AccessToken, attributes, options);
            Assert.AreEqual(user, updateResult.Email);
            Assert.IsNotNull(updateResult.UserMetadata);
        }

        [TestMethod("StatelessClient: Returns current user")]
        public async Task GetUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            var session = await SignUp(user, password, options);

            Assert.AreEqual(user, session.User.Email);
        }

        [TestMethod("StatelessClient: Throws Exception on Invalid Username and Password")]
        public async Task SignsInUserWrongPassword()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await SignUp(user, password, options);

            await Assert.ThrowsExceptionAsync<BadRequestException>(async () =>
            {
                var result = await SignIn(user, password + "$", options);
            });
        }

        [TestMethod("StatelessClient: Sends Invite Email")]
        public async Task SendsInviteEmail()
        {
            var user = $"{RandomString(12)}@supabase.io";
            var service_role_key = GenerateServiceRoleToken();
            var result = await InviteUserByEmail(user, service_role_key, options);
            Assert.IsTrue(result);
        }

        [TestMethod("StatelessClient: Deletes User")]
        public async Task DeletesUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            var session = await SignUp(user, password, options);
            var uid = session.User.Id;

            var service_role_key = GenerateServiceRoleToken();
            var result = await DeleteUser(uid, service_role_key, options);

            Assert.IsTrue(result);
        }

        [TestMethod("StatelessClient: Sends Reset Password Email")]
        public async Task ClientSendsResetPasswordForEmail()
        {
            var email = $"{RandomString(12)}@supabase.io";
            await SignUp(email, password, options);
            var result = await ResetPasswordForEmail(email, options);
            Assert.IsTrue(result);
        }

        [TestMethod("Client: Lists users")]
        public async Task ClientListUsers()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await ListUsers(service_role_key, options);

            Assert.IsTrue(result.Users.Count > 0);
        }

        [TestMethod("Client: Lists users pagination")]
        public async Task ClientListUsersPagination()
        {
            var service_role_key = GenerateServiceRoleToken();

            var page1 = await ListUsers(service_role_key, options, page: 1, perPage: 1);
            var page2 = await ListUsers(service_role_key, options, page: 2, perPage: 1);

            Assert.AreEqual(page1.Users.Count, 1);
            Assert.AreEqual(page2.Users.Count, 1);
            Assert.AreNotEqual(page1.Users[0].Id, page2.Users[0].Id);
        }

        [TestMethod("Client: Lists users sort")]
        public async Task ClientListUsersSort()
        {
            var service_role_key = GenerateServiceRoleToken();

            var result1 = await ListUsers(service_role_key, options, sortBy: "created_at", sortOrder: SortOrder.Descending);
            var result2 = await ListUsers(service_role_key, options, sortBy: "created_at", sortOrder: SortOrder.Ascending);

            Assert.AreNotEqual(result1.Users[0].Id, result2.Users[0].Id);
        }

        [TestMethod("Client: Lists users filter")]
        public async Task ClientListUsersFilter()
        {
            var service_role_key = GenerateServiceRoleToken();

            var result1 = await ListUsers(service_role_key, options, filter: "@nonexistingrandomemailprovider.com");
            var result2 = await ListUsers(service_role_key, options, filter: "@supabase.io");

            Assert.AreNotEqual(result2.Users.Count, 0);
            Assert.AreEqual(result1.Users.Count, 0);
            Assert.AreNotEqual(result1.Users.Count, result2.Users.Count);
        }

        [TestMethod("Client: Get User by Id")]
        public async Task ClientGetUserById()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await ListUsers(service_role_key, options, page: 1, perPage: 1);

            var userResult = result.Users[0];
            var userByIdResult = await GetUserById(service_role_key, options, userResult.Id);

            Assert.AreEqual(userResult.Id, userByIdResult.Id);
            Assert.AreEqual(userResult.Email, userByIdResult.Email);
        }

        [TestMethod("Client: Create a user")]
        public async Task ClientCreateUser()
        {
            var service_role_key = GenerateServiceRoleToken();
            var result = await CreateUser(service_role_key, options, $"{RandomString(12)}@supabase.io", password);

            Assert.IsNotNull(result);

            var attributes = new AdminUserAttributes
            {
                UserMetadata = new Dictionary<string, object> { { "firstName", "123" } },
            };

            var result2 = await CreateUser(service_role_key, options, $"{RandomString(12)}@supabase.io", password, attributes);
            Assert.AreEqual("123", result2.UserMetadata["firstName"]);

            var result3 = await CreateUser(service_role_key, options, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io", Password = password });
            Assert.IsNotNull(result3);
        }

        [TestMethod("Client: Update User by Id")]
        public async Task ClientUpdateUserById()
        {
            var service_role_key = GenerateServiceRoleToken();
            var createdUser = await CreateUser(service_role_key, options, $"{RandomString(12)}@supabase.io", password);

            Assert.IsNotNull(createdUser);

            var updatedUser = await UpdateUserById(service_role_key, options, createdUser.Id, new AdminUserAttributes { Email = $"{RandomString(12)}@supabase.io" });

            Assert.IsNotNull(updatedUser);

            Assert.AreEqual(createdUser.Id, updatedUser.Id);
            Assert.AreNotEqual(createdUser.Email, updatedUser.Email);
        }
    }
}
