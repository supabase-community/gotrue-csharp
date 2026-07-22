#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GotrueTests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase.Gotrue;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

#endregion

namespace GotrueTests;

/// <summary>
///     Contract tests for the diagnostics the SDK emits through System.Diagnostics
///     (ActivitySource/Meter "Supabase.Gotrue") and for the sanitization rule: telemetry and
///     debug output must never contain a token, JWT, query string, or other secret.
/// </summary>
[TestClass]
[TestCategory("Contract")]
public class ObservabilityContractTests
{
	private const string AccessToken = "an-access-token";
	private const string RefreshTokenValue = "a-refresh-token";

	private readonly List<Activity> activities = new();
	private readonly List<KeyValuePair<double, Dictionary<string, object?>>> measurements = new();
	private ActivityListener activityListener;
	private Client client;
	private MeterListener meterListener;
	private MockGotrueServer server;

	[TestInitialize]
	public void TestInitializer()
	{
		server = new MockGotrueServer();
		client = new Client(new ClientOptions
		{
			Url = server.Url,
			AutoRefreshToken = true,
			AllowUnconfirmedUserSessions = false,
			Headers = new Dictionary<string, string> { { "apikey", MockGotrueServer.ApiKey } }
		});

		activityListener = new ActivityListener
		{
			ShouldListenTo = source => source.Name == GotrueDiagnostics.SourceName,
			Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
			ActivityStopped = activity => activities.Add(activity)
		};
		ActivitySource.AddActivityListener(activityListener);

		meterListener = new MeterListener
		{
			InstrumentPublished = (instrument, listener) =>
			{
				if (instrument.Meter.Name == GotrueDiagnostics.SourceName)
					listener.EnableMeasurementEvents(instrument);
			}
		};
		meterListener.SetMeasurementEventCallback<double>((_, value, tags, _) =>
		{
			var tagValues = new Dictionary<string, object?>();
			foreach (var tag in tags)
				tagValues[tag.Key] = tag.Value;
			lock (measurements)
			{
				measurements.Add(new KeyValuePair<double, Dictionary<string, object?>>(value, tagValues));
			}
		});
		meterListener.Start();
	}

	[TestCleanup]
	public void TestCleanup()
	{
		client.Shutdown();
		activityListener.Dispose();
		meterListener.Dispose();
		server.Dispose();
	}

	[TestMethod(DisplayName = "The HTTP span records the request URL without its query string")]
	public async Task HttpSpanRecordsSanitizedUrl()
	{
		MockTokenSuccess();
		await Refresh();
		HttpTokenSpan().GetTagItem("url.full").Should().Be($"{server.Url}/token",
			"the query string carries grant types and credentials and must never be recorded");
	}

	[TestMethod(DisplayName = "The HTTP span follows OpenTelemetry HTTP client conventions")]
	public async Task HttpSpanRecordsMethodAndStatusCode()
	{
		MockTokenSuccess();
		await Refresh();
		var httpSpan = HttpTokenSpan();
		httpSpan.Kind.Should().Be(ActivityKind.Client);
		httpSpan.GetTagItem("http.request.method").Should().Be("POST");
		httpSpan.GetTagItem("http.response.status_code").Should().Be(200);
	}

	[TestMethod(DisplayName = "A public operation emits a domain span that parents the HTTP span")]
	public async Task DomainSpanIsEmittedAndParentsTheHttpSpan()
	{
		MockTokenSuccess();
		await Refresh();
		var domainSpan = activities.Should().ContainSingle(a => a.OperationName == "gotrue.refresh_token").Which;
		HttpTokenSpan().ParentSpanId.Should().Be(domainSpan.SpanId);
	}

	[TestMethod(DisplayName = "A failed HTTP request marks the span as an error")]
	public async Task FailedHttpRequestMarksTheSpanAsError()
	{
		MockTokenFailure();
		await FluentActions.Awaiting(Refresh).Should().ThrowAsync<Exception>();
		var httpSpan = HttpTokenSpan();
		httpSpan.Status.Should().Be(ActivityStatusCode.Error);
		httpSpan.GetTagItem("http.response.status_code").Should().Be(500);
	}

	[TestMethod(DisplayName = "The request duration histogram is recorded per HTTP request")]
	public async Task RequestDurationMetricIsRecorded()
	{
		MockTokenSuccess();
		await Refresh();
		meterListener.RecordObservableInstruments();
		var measurement = measurements.Should().ContainSingle().Which;
		measurement.Key.Should().BeGreaterThan(0);
		measurement.Value.Should().Contain("http.response.status_code", 200);
		measurement.Value.Should().Contain("url.path", "/token");
	}

	[TestMethod(DisplayName = "Telemetry never contains the session tokens")]
	public async Task TelemetryDoesNotLeakTokens()
	{
		MockTokenSuccess();
		await Refresh();
		var tagValues = activities
			.SelectMany(a => a.TagObjects)
			.Select(tag => tag.Value?.ToString() ?? "")
			.Concat(measurements.SelectMany(m => m.Value.Values).Select(v => v?.ToString() ?? ""))
			.Concat(activities.Select(a => a.DisplayName));
		tagValues.Should().OnlyContain(value =>
			!value.Contains(AccessToken) && !value.Contains(RefreshTokenValue) &&
			!value.Contains("new-access-token") && !value.Contains("new-refresh-token"));
	}

	[TestMethod(DisplayName = "The debug log never contains the session tokens when a refresh fails")]
	public async Task DebugLogDoesNotLeakTokensWhenRefreshFails()
	{
		var messages = new List<string>();
#pragma warning disable CS0618 // the obsolete debug surface stays leak-free until it is removed in v8
		client.AddDebugListener((message, _) => messages.Add(message));
#pragma warning restore CS0618
		MockTokenSuccess();
		await Refresh();
		server.Reset();
		MockTokenFailure();
		var session = await client.RetrieveSessionAsync();
		session.Should().BeNull("the failed refresh destroys the session");
		messages.Should().NotBeEmpty("the failed refresh should be reported to debug listeners");
		messages.Should().OnlyContain(message =>
				!message.Contains("new-access-token") && !message.Contains("new-refresh-token"),
			"debug output must never contain the session's access or refresh token");
	}

	private Task Refresh() => client.RefreshToken(AccessToken, RefreshTokenValue);

	private Activity HttpTokenSpan() =>
		activities.Should().ContainSingle(a => a.OperationName == "POST /token").Which;

	private void MockTokenSuccess() =>
		server
			.Given(Request.Create().WithPath("/token").UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBody(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TokenRefresh", "Fixtures", "token_success.json"))));

	private void MockTokenFailure() =>
		server
			.Given(Request.Create().WithPath("/token").UsingPost())
			.RespondWith(Response.Create()
				.WithStatusCode(500)
				.WithHeader("Content-Type", "application/json")
				.WithBody("{\"msg\":\"refresh exploded\"}"));
}