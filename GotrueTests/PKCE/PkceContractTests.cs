#region

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GotrueTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using static Supabase.Gotrue.Constants;

#endregion

namespace GotrueTests.PKCE;

[TestClass]
[TestCategory("Contract")]
public class PkceContractTests
{
	private IGotrueClient<User, Session> client;
	private MockGotrueServer server;

	[TestInitialize]
	public void TestInitialize()
	{
		server = new MockGotrueServer();
		client = TestClients.Against(server);
	}

	[TestCleanup]
	public void TestCleanup() => server.Dispose();

	[TestMethod("SignInWithOtp (email, PKCE): returns raw nonce as verifier and sends its SHA-256 hash as code_challenge")]
	public async Task SignInWithOtpPkce_ChallengeIsHashOfVerifier()
	{
		server
			.Given(Request.Create()
				.WithPath("/otp")
				.UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{}"));
		var state = await client.SignInWithOtp(new SignInWithPasswordlessEmailOptions("test@example.com")
		{
			FlowType = OAuthFlowType.PKCE
		});
		
		var request = server.VerifySingleReceivedRequest().WithMethod(HttpMethod.Post).WithPath("/otp");
		request.ReadJsonBodyField("code_challenge_method").Should().Be("s256");
		request.ReadJsonBodyField("code_challenge").Should().Be(S256(state.PKCEVerifier!), "server must receive BASE64URL(SHA256(verifier)) per RFC 7636 §4.2");
	}

	[TestMethod("ResetPasswordForEmail (PKCE): returns raw nonce as verifier and sends its SHA-256 hash as code_challenge")]
	public async Task ResetPasswordForEmailPkce_ChallengeIsHashOfVerifier()
	{
		server
			.Given(Request.Create()
				.WithPath("/recover")
				.UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{}"));
		var state = await client.ResetPasswordForEmail(new ResetPasswordForEmailOptions("test@example.com")
		{
			FlowType = OAuthFlowType.PKCE
		});

		var request = server.VerifySingleReceivedRequest().WithMethod(HttpMethod.Post).WithPath("/recover");
		request.ReadJsonBodyField("code_challenge_method").Should().Be("s256");
		request.ReadJsonBodyField("code_challenge").Should().Be(S256(state.PKCEVerifier!), "server must receive BASE64URL(SHA256(verifier)) per RFC 7636 §4.2");
	}

	private static string S256(string verifier)
	{
		using var sha = SHA256.Create();
		var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(verifier));
		return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}
}
