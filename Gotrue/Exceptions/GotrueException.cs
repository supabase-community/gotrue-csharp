using System;
using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
	/// <summary>
	/// Errors from the GoTrue server are wrapped by this exception
	/// </summary>
	public class GotrueException : Exception
	{
		public GotrueException(string? message) : base(message) { }
		public GotrueException(string? message, Exception? innerException) : base(message, innerException) { }

		public HttpResponseMessage? Response { get; internal set; }

		public string? Content { get; internal set; }

		public int StatusCode { get; internal set; }
		public void AddReason()
		{
			Reason = FailureHint.DetectReason(this);
			//Debug.WriteLine(Content);
		}
		public FailureHint.Reason Reason { get; private set; }
	}
}
