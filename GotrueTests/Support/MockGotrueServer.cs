using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace GotrueTests.Support
{
	internal sealed class MockGotrueServer : IDisposable
	{
		internal const string ApiKey = "test-api-key";

		private readonly WireMockServer server = WireMockServer.Start();

		internal string Url => server.Url!;

		internal IRespondWithAProvider Given(IRequestBuilder requestBuilder) =>
			server.Given(requestBuilder);

		internal ReceivedRequest VerifySingleReceivedRequest()
		{
			var entry = server.LogEntries.Should().ContainSingle("the SDK should emit exactly one request").Which;
			return new ReceivedRequest(entry.RequestMessage);
		}

		public void Dispose() => server.Stop();
	}

	/// <summary>
	/// Fluent assertions over a request the SDK sent to the <see cref="MockGotrueServer"/>.
	/// </summary>
	internal sealed class ReceivedRequest
	{
		private readonly IRequestMessage request;

		internal ReceivedRequest(IRequestMessage request) => this.request = request;

		internal ReceivedRequest WithPath(string path)
		{
			request.Path.Should().Be(path);
			return this;
		}

		internal ReceivedRequest WithMethod(HttpMethod method)
		{
			request.Method.Should().Be(method.ToString());
			return this;
		}

		internal ReceivedRequest WithQueryParam(string name, string expected)
		{
			request.Query.Should().ContainKey(name).WhoseValue.Single().Should().Be(expected);
			return this;
		}

		internal ReceivedRequest WithHeader(string name, string expected)
		{
			request.Headers.Should().ContainKey(name).WhoseValue.Single().Should().Be(expected);
			return this;
		}

		internal ReceivedRequest WithJsonContentType()
		{
			request.Headers.Should().ContainKey("Content-Type").WhoseValue.Single().Should().StartWith("application/json");
			return this;
		}

		internal void WithExactJsonBody(string field, string expected)
		{
			request.Body.Should().NotBeNull("the request should have a body");
			var body = JObject.Parse(request.Body);
			((string)body[field]).Should().Be(expected);
		}

		internal string ReadJsonBodyField(string field)
		{
			request.Body.Should().NotBeNull("the request should have a body");
			var body = JObject.Parse(request.Body);
			return (string)body[field];
		}
	}
}