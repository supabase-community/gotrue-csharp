using System;
using System.Diagnostics;
using System.Net.Http;
using Supabase.Gotrue.Responses;

namespace Supabase.Gotrue.Exceptions
{

	public class GotrueException : Exception
	{
		public GotrueException(string? message) : base(message) { }
		public GotrueException(string? message, Exception? innerException) : base(message, innerException) { }

		public HttpResponseMessage? Response { get; internal set; }

		public string? Content { get; internal set; }

		public int StatusCode { get; set; }
		public void AddReason()
		{
			Reason = FailureHint.DetectReason(this);
			Debug.WriteLine(Content);
		}
		public FailureHint.Reason Reason { get; private set; }
	}
}
