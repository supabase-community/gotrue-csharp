using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GotrueTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static GotrueTests.TestUtils;
using static Supabase.Gotrue.Constants.AuthState;
using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace GotrueTests.TokenRefresh;

[TestClass]
[TestCategory("Behavior")]
public class RefreshBehaviorTests
{
	private readonly List<Constants.AuthState> stateChanges = [];
	private IGotrueClient<User, Session> client;
	private TestSessionPersistence persistence;

	[TestInitialize]
	public void TestInitializer()
	{
		persistence = new TestSessionPersistence();
		client = TestClients.AgainstCliStack();
		client.SetPersistence(persistence);
		client.AddDebugListener(LogDebug);
		client.AddStateChangedListener((_, state) => stateChanges.Add(state));
	}

	[TestMethod("Refreshing a session rotates the refresh token and the server accepts the new access token")]
	public async Task RefreshRotatesToken()
	{
		var signUp = await SignUpNewUser();
		var refreshed = await client.RefreshSession();
		await VerifyRotatedSession(signUp, refreshed);
	}

	[TestMethod("Refreshing an expired session succeeds")]
	public async Task RefreshSucceedsWhenExpired()
	{
		var signUp = await SignUpNewUser();
		ExpireCurrentSession();
		var refreshed = await client.RefreshSession();
		await VerifyRotatedSession(signUp, refreshed);
	}

	[DataTestMethod("Refreshing with a rejected refresh token throws InvalidRefreshToken and destroys the session")]
	[DataRow("bogus-token", DisplayName = "Malformed token")]
	[DataRow("abcdef012345", DisplayName = "Well-formed unknown token")]
	public async Task RefreshFailsWithRejectedToken(string rejectedToken)
	{
		await SignUpNewUser();
		client.CurrentSession!.RefreshToken = rejectedToken;
		Func<Task> refresh = () => client.RefreshSession();
		var exception = await refresh.Should().ThrowAsync<GotrueException>();
		exception.Which.Reason.Should().Be(InvalidRefreshToken);
		client.CurrentSession.Should().BeNull();
	}

	private sealed record SignUpResult(Session Session, string UserId);

	private async Task<SignUpResult> SignUpNewUser()
	{
		var session = await client.SignUp($"{RandomString(12)}@supabase.io", PASSWORD);
		session.Should().NotBeNull();
		session!.User?.Id.Should().NotBeNullOrEmpty();
		return new SignUpResult(session, session.User!.Id!);
	}

	private void ExpireCurrentSession()
	{
		client.CurrentSession.Should().NotBeNull();
		client.CurrentSession!.CreatedAt = DateTime.UtcNow.AddDays(-1);
	}

	private async Task VerifyRotatedSession(SignUpResult signUp, Session refreshed)
	{
		refreshed.Should().NotBeNull();
		refreshed.RefreshToken.Should().NotBe(signUp.Session.RefreshToken);
		stateChanges.Should().Contain(TokenRefreshed);
		persistence.SavedSession.Should().BeSameAs(client.CurrentSession);
		var user = await client.GetUser(refreshed.AccessToken!);
		user.Should().NotBeNull();
		user!.Id.Should().Be(signUp.UserId);
	}
}
