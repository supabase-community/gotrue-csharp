using System;
using System.Linq;
using System.Threading.Tasks;
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestInitialize]
        public async Task TestInitializer()
        {
            client = await Initialize();
        }

        [TestMethod("Client: Signs Up User")]
        public async Task ClientSignsUpUser()
        {
            var result = await client.SignUp($"{RandomString(12)}@supabase.io", password);

            Assert.IsNotNull(result.AccessToken);
            Assert.IsNotNull(result.RefreshToken);
            Assert.IsInstanceOfType(result.User, typeof(User));
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

        [TestMethod("Client: Returns Auth Url for Provider")]
        public async Task ClientReturnsAuthUrlForProvider()
        {
            var result = await client.SignIn(Provider.Google);
            Assert.AreEqual("http://localhost:9999/authorize?provider=google", result);
        }
    }
}
