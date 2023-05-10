using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace Supabase.Gotrue.Exceptions
{
	/// <summary>
	/// Maps Supabase server errors to hints based on the status code and the contents of the error message.
	/// </summary>
	public static class FailureHint
	{
		public enum Reason
		{
			Unknown,
			UserBadPassword,
			UserBadEmailAddress,
			UserMissingInformation,
			UserAlreadyRegistered,
			InvalidRefreshToken,
			AdminTokenRequired
		}

		public static Reason DetectReason(GotrueException gte)
		{
			if (gte.Content == null)
				return Unknown;

			return gte.StatusCode switch
			{
				400 when gte.Content.Contains("User already registered") => UserAlreadyRegistered,
				400 when gte.Content.Contains("Invalid Refresh Token") => InvalidRefreshToken,
				401 when gte.Content.Contains("This endpoint requires a Bearer token") => AdminTokenRequired,
				422 when gte.Content.Contains("Password should be at least") => UserBadPassword,
				422 when gte.Content.Contains("Signup requires a valid password") => UserBadPassword,
				422 when gte.Content.Contains("Unable to validate email address") => UserBadEmailAddress,
				422 when gte.Content.Contains("provide your email or phone number") => UserMissingInformation,
				_ => Unknown
			};

		}
	}
}
