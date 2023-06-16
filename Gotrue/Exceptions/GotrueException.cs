﻿using System;
using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
	/// <summary>
	/// Errors from the GoTrue server are wrapped by this exception
	/// </summary>
	public class GotrueException : Exception
	{
		/// <summary>
		/// Something with wrong with Gotrue / Auth
		/// </summary>
		/// <param name="message">Short description of the error source</param>
		public GotrueException(string? message) : base(message) { }
		/// <summary>
		/// Something with wrong with Gotrue / Auth
		/// </summary>
		/// <param name="message">Short description of the error source</param>
		/// <param name="innerException">The underlying exception</param>
		public GotrueException(string? message, Exception? innerException) : base(message, innerException) { }
		/// <summary>
		/// Something with wrong with Gotrue / Auth
		/// </summary>
		/// <param name="message">Short description of the error source</param>
		/// <param name="reason">Best effort attempt to detect the reason for the failure</param>
		public GotrueException(string? message, FailureHint.Reason reason) : base(message)
		{
			Reason = reason;
		}
		/// <summary>
		/// Something with wrong with Gotrue / Auth
		/// </summary>
		/// <param name="message">Short description of the error source</param>
		/// <param name="reason">Assigned reason</param>
		/// <param name="innerException"></param>
		public GotrueException(string message, FailureHint.Reason reason, Exception? innerException) : base(message, innerException)
		{
			Reason = reason;
		}

		/// <summary>
		/// The HTTP response from the server
		/// </summary>
		public HttpResponseMessage? Response { get; internal set; }

		/// <summary>
		/// The content of the HTTP response from the server
		/// </summary>
		public string? Content { get; internal set; }

		/// <summary>
		/// The HTTP status code from the server
		/// </summary>
		public int StatusCode { get; internal set; }

		/// <summary>
		/// Adds the best-effort reason for the failure 
		/// </summary>
		public void AddReason()
		{
			Reason = FailureHint.DetectReason(this);
			//Debug.WriteLine(Content);
		}

		/// <summary>
		/// Best guess at what caused the error from the server, see <see cref="FailureHint.Reason"/>
		/// </summary>
		public FailureHint.Reason Reason { get; private set; }
	}
}
