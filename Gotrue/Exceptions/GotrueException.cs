using System;
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
		
		public ErrorResponse? Error { get; private set; }
	}
}
