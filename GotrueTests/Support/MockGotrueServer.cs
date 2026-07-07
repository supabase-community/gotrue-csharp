using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;

namespace GotrueTests.Support
{
	internal sealed class MockGotrueServer : IDisposable
	{
		internal const string ApiKey = "test-api-key";

		private readonly WireMockServer server = WireMockServer.Start();

		internal string Url => server.Url!;

		internal void RespondsTo(string path, int statusCode, string jsonBody) =>
			server
				.Given(Request.Create().WithPath(path))
				.RespondWith(Response.Create()
					.WithStatusCode(statusCode)
					.WithHeader("Content-Type", "application/json")
					.WithBody(jsonBody));

		internal ReceivedRequest VerifySingleReceivedRequest()
		{
			AreEqual(1, server.LogEntries.Count, "expected the SDK to emit exactly one request");
			return new ReceivedRequest(server.LogEntries.Single().RequestMessage);
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
			AreEqual(path, request.Path);
			return this;
		}


		internal ReceivedRequest WithMethod(HttpMethod method)
		{
			AreEqual(method.ToString(), request.Method);
			return this;
		}


		internal ReceivedRequest WithQueryParam(string name, string expected)
		{
			IsTrue(request.Query != null && request.Query.ContainsKey(name), $"missing query parameter '{name}'");
			AreEqual(expected, request.Query[name].Single());
			return this;
		}

		internal ReceivedRequest WithHeader(string name, string expected)
		{
			IsTrue(request.Headers != null && request.Headers.ContainsKey(name), $"missing header '{name}'");
			AreEqual(expected, request.Headers[name].Single());
			return this;
		}

		internal ReceivedRequest WithJsonContentType()
		{
			IsTrue(request.Headers != null && request.Headers.ContainsKey("Content-Type"), "missing Content-Type header");
			StartsWith(request.Headers["Content-Type"].Single(), "application/json");
			return this;
		}

		internal void WithExactJsonBody(string field, string expected)
		{
			IsNotNull(request.Body, "request has no body");
			var body = JObject.Parse(request.Body);
			AreEqual(expected, (string)body[field]);
		}
	}
}