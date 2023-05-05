using static Supabase.Gotrue.Exceptions.FailureHint.Reason;

namespace Supabase.Gotrue.Exceptions
{
	public static class FailureHint
	{
		public enum Reason
		{
			Unknown,
			BadPassword,
			BadEmailAddress,
			MissingInformation,
			AlreadyRegistered,
			InvalidRefreshToken
		}

		public static Reason DetectReason(GotrueException gte)
		{
			if (gte.Content == null)
				return Unknown;

			return gte.StatusCode switch
			{
				400 when gte.Content.Contains("User already registered") => AlreadyRegistered,
				400 when gte.Content.Contains("Invalid Refresh Token") => InvalidRefreshToken,
				422 when gte.Content.Contains("Password should be at least") => BadPassword,
				422 when gte.Content.Contains("Unable to validate email address") => BadEmailAddress,
				422 when gte.Content.Contains("provide your email or phone number") => MissingInformation,
				_ => Unknown
			};

		}
	}
}
