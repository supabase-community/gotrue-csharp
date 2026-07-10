#region

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GotrueTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

#endregion

namespace GotrueTests.TokenRefresh;

[TestClass]
[TestCategory("Contract")]
public class RefreshContractTests
{
	private const string AccessToken = "an-access-token";
	private const string RefreshTokenValue = "a-refresh-token";
	private IGotrueClient<User, Session> client;
	private MockGotrueServer server;

	[TestInitialize]
	public void TestInitializer()
	{
		server = new MockGotrueServer();
		client = TestClients.Against(server);
	}

	[TestCleanup]
	public void TestCleanup() => server.Dispose();

	[TestMethod("Refresh sends POST /token?grant_type=refresh_token with bearer auth, the api key, and only the refresh token as body")]
	public async Task RefreshSendsExpectedRequest()
	{
		MockSuccessResponse();
		await client.RefreshToken(AccessToken, RefreshTokenValue);
		server.VerifySingleReceivedRequest()
			.WithMethod(HttpMethod.Post)
			.WithPath("/token")
			.WithQueryParam("grant_type", "refresh_token")
			.WithHeader("Authorization", $"Bearer {AccessToken}")
			.WithHeader("apikey", MockGotrueServer.ApiKey)
			.WithJsonContentType()
			.WithExactJsonBody("refresh_token", RefreshTokenValue);
	}

	[TestMethod("A successful refresh response becomes the current session")]
	public async Task RefreshMapsResponseToSession()
	{
		MockSuccessResponse();
		await client.RefreshToken(AccessToken, RefreshTokenValue);
		client.CurrentSession.Should().NotBeNull();
		client.CurrentSession!.AccessToken.Should().Be("new-access-token");
		client.CurrentSession!.RefreshToken.Should().Be("new-refresh-token");
		client.CurrentSession!.User.Should().NotBeNull();
		client.CurrentSession!.User?.Id.Should().Be("user-id-123");
		client.CurrentSession!.ExpiresIn.Should().Be(3600);
	}

	[DataTestMethod("A 400 invalid-refresh-token response is classified as InvalidRefreshToken and destroys the session")]
	[DataRow("token_not_found_error.json", DisplayName = "unknown token (refresh_token_not_found)")]
	[DataRow("malformed_token_error.json", DisplayName = "malformed token (validation_failed)")]
	public async Task InvalidRefreshTokenResponseIsClassified(string fixture)
	{
		MockErrorResponse(400, Fixture(fixture));
		var refresh = () => client.RefreshToken(AccessToken, RefreshTokenValue);
		var exception = await refresh.Should().ThrowAsync<GotrueException>();
		exception.Which.Reason.Should().Be(InvalidRefreshToken);
		client.CurrentSession.Should().BeNull();
	}

	[TestMethod("An unrecognized error response is classified as Unknown")]
	public async Task UnrecognizedErrorResponseIsUnknown()
	{
		MockErrorResponse(500, Fixture("unclassified_error.json"));
		var refresh = () => client.RefreshToken(AccessToken, RefreshTokenValue);
		var exception = await refresh.Should().ThrowAsync<GotrueException>();
		exception.Which.Reason.Should().Be(Unknown);
		client.CurrentSession.Should().BeNull();
	}

	private void MockSuccessResponse() =>
		server
			.Given(Request.Create().WithPath("/token").UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody(Fixture("token_success.json")));

	private void MockErrorResponse(int statusCode, string body) =>
		server
			.Given(Request.Create().WithPath("/token").UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(statusCode)
				.WithHeader("Content-Type", "application/json")
				.WithBody(body));

	private static string Fixture(string name) =>
		File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TokenRefresh", "Fixtures", name));
}
