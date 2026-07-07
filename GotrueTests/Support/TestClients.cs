using System;
using System.Collections.Generic;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace GotrueTests.Support
{
	/// <summary>
	/// One place to build a Gotrue client per test tier: contract tests get a client pointed
	/// at a <see cref="MockGotrueServer"/>, behavior tests get one pointed at the local
	/// Supabase CLI stack (overridable with SUPABASE_CLI_AUTH_URL / SUPABASE_CLI_ANON_KEY).
	/// </summary>
	internal static class TestClients
	{
		private static readonly string CliAuthUrl =
			Environment.GetEnvironmentVariable("SUPABASE_CLI_AUTH_URL") ?? "http://127.0.0.1:54321/auth/v1";

		private static readonly string CliAnonKey =
			Environment.GetEnvironmentVariable("SUPABASE_CLI_ANON_KEY") ??
			"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6ImFub24iLCJleHAiOjE5ODM4MTI5OTZ9.CRXP1A7WOeoJeXxjNni43kdQwgnWNReilDMblYTn_I0";

		internal static IGotrueClient<User, Session> AgainstCliStack() =>
			Build(CliAuthUrl, CliAnonKey, autoRefreshToken: true, allowUnconfirmedUserSessions: true);

		internal static IGotrueClient<User, Session> Against(MockGotrueServer server) =>
			Build(server.Url, MockGotrueServer.ApiKey, autoRefreshToken: false, allowUnconfirmedUserSessions: false);

		private static IGotrueClient<User, Session> Build(string url, string apiKey, bool autoRefreshToken, bool allowUnconfirmedUserSessions) =>
			new Client(new ClientOptions
			{
				Url = url,
				AutoRefreshToken = autoRefreshToken,
				AllowUnconfirmedUserSessions = allowUnconfirmedUserSessions,
				Headers = new Dictionary<string, string> { { "apikey", apiKey } }
			});
	}
}
