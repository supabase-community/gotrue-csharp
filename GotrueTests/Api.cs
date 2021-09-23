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

namespace GotrueTests
{
    [TestClass]
    public class Api
    {
        private Client client;

        private string password = "I@M@SuperP@ssWord";

        private static Random random = new Random();


        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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
            var email = $"{RandomString(12)}@supabase.io";
            var result = await client.SignUp(email, password);

            Assert.IsNotNull(result.AccessToken);
            Assert.IsNotNull(result.RefreshToken);
            Assert.IsInstanceOfType(result.User, typeof(User));
        }

        [TestMethod("Client: Signs Up the same user twice should throw an error")]
        public async Task ClientSignsUpUserTwiceShouldThrowError()
        {
            var email = $"{RandomString(12)}@supabase.io";
            await client.SignUp(email, password);
            await Assert.ThrowsExceptionAsync<BadRequestException>(async () =>
            {
                var result = await client.SignUp(email, password);
            });

        }

        [TestMethod("Client: Signs In User with Email & Password")]
        public async Task ClientSignsInUser()
        {
            var user = $"{RandomString(12)}@supabase.io";
            await client.SignUp(user, password);

            await client.SignOut();

            var result = await client.SignIn(user, password);

            Assert.IsNotNull(result.AccessToken);
            Assert.IsNotNull(result.RefreshToken);
            Assert.IsInstanceOfType(result.User, typeof(User));
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
    }
}
