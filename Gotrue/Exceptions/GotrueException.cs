using System;

namespace Supabase.Gotrue.Exceptions
{
	public class GotrueException : Exception
	{
		public GotrueException(string? message) : base(message) { }
		public GotrueException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
