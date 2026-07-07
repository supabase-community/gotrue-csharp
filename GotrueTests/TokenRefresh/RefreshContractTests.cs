#region

using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GotrueTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

#endregion

namespace GotrueTests.TokenRefresh;

[TestClass]
[TestCategory("Contract")]
public class RefreshContractTests
{
	private const string AccessToken = "an-access-token";
	private const string RefreshTokenValue = "a-refresh-token";
	private const string UnclassifiedError = "{\"msg\":\"internal server error\"}";
	private const string TokenNotFoundError = "{\"code\":400,\"error_code\":\"refresh_token_not_found\",\"msg\":\"Invalid Refresh Token: Refresh Token Not Found\"}";
	private const string MalformedTokenError = "{\"code\":400,\"error_code\":\"validation_failed\",\"msg\":\"Refresh token is not valid\"}";
	private IGotrueClient<User, Session> client;
	private MockGotrueServer server;

	[TestInitialize]
	public void TestInitializer()
	{
		server = new MockGotrueServer();
		client = TestClients.Against(server);
	}

	[TestCleanup]
	public void TestCleanup()
	{
		server.Dispose();
	}

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
		ValidateCurrentSessionPropertiesVerifyRefreshedToken();
	}


	[DataTestMethod("A 400 invalid-refresh-token response is classified as InvalidRefreshToken and destroys the session")]
	[DataRow(TokenNotFoundError, DisplayName = "unknown token (refresh_token_not_found)")]
	[DataRow(MalformedTokenError, DisplayName = "malformed token (validation_failed)")]
	public async Task InvalidRefreshTokenResponseIsClassified(string errorBody)
	{
		MockErrorResponse(400, errorBody);
		var refresh = () => client.RefreshToken(AccessToken, RefreshTokenValue);
		var exception = await refresh.Should().ThrowAsync<GotrueException>();
		exception.Which.Reason.Should().Be(InvalidRefreshToken);
		client.CurrentSession.Should().BeNull();
	}

	[TestMethod("An unrecognized error response is classified as Unknown")]
	public async Task UnrecognizedErrorResponseIsUnknown()
	{
		MockErrorResponse(500, UnclassifiedError);
		var refresh = () => client.RefreshToken(AccessToken, RefreshTokenValue);
		var exception = await refresh.Should().ThrowAsync<GotrueException>();
		exception.Which.Reason.Should().Be(Unknown);
		client.CurrentSession.Should().BeNull();
	}

	private void MockSuccessResponse() =>
		server.RespondsTo("/token", 200,
			"{\"access_token\":\"new-access-token\",\"refresh_token\":\"new-refresh-token\",\"token_type\":\"bearer\",\"expires_in\":3600,\"user\":{\"id\":\"user-id-123\",\"aud\":\"authenticated\"}}");

	private void MockErrorResponse(int statusCode, string body) =>
		server.RespondsTo("/token", statusCode, body);
	
	private void ValidateCurrentSessionPropertiesVerifyRefreshedToken()
	{
		client.CurrentSession.Should().NotBeNull();
		client.CurrentSession!.AccessToken.Should().Be("new-access-token");
		client.CurrentSession!.RefreshToken.Should().Be("new-refresh-token");
		client.CurrentSession!.User.Should().NotBeNull();
		client.CurrentSession!.User?.Id.Should().Be("user-id-123");
		client.CurrentSession!.ExpiresIn.Should().Be(3600);
	}
}