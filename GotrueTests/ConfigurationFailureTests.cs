using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using static GotrueTests.TestUtils;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace GotrueTests
{
	[TestClass]
	public class ConfigurationFailureTests
	{
		[TestMethod("Bad URL message")]
		public async Task BadUrlTest()
		{
			var client = new Client(new ClientOptions { Url = "https://badprojecturl.supabase.co", AllowUnconfirmedUserSessions = true });
			client.AddDebugListener(LogDebug);

			var email = $"{RandomString(12)}@supabase.io";
			await ThrowsExceptionAsync<HttpRequestException>(async () =>
			{
				await client.SignUp(email, PASSWORD);
			});
		}

		[TestMethod("Bad service key message")]
		public async Task BadServiceApiKeyTest()
		{
			var client = new Client(new ClientOptions { AllowUnconfirmedUserSessions = true });
			client.AddDebugListener(LogDebug);

			var x = await ThrowsExceptionAsync<GotrueException>(async () =>
			{
				await client.ListUsers("garbage key");
			});
			AreEqual(AdminTokenRequired, x.Reason);
		}
	}
}
