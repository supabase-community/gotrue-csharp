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
			UserEmailNotConfirmed,
			UserBadMultiple,
			UserBadPassword,
			UserBadEmailAddress,
			UserBadPhoneNumber,
			UserMissingInformation,
			UserAlreadyRegistered,
			UserTooManyRequests,
			InvalidRefreshToken,
			AdminTokenRequired
		}

		public static Reason DetectReason(GotrueException gte)
		{
			if (gte.Content == null)
				return Unknown;

			return gte.StatusCode switch
			{
				400 when gte.Content.Contains("Email not confirmed") => UserEmailNotConfirmed,
				400 when gte.Content.Contains("User already registered") => UserAlreadyRegistered,
				400 when gte.Content.Contains("Invalid Refresh Token") => InvalidRefreshToken,
				401 when gte.Content.Contains("This endpoint requires a Bearer token") => AdminTokenRequired,
				422 when gte.Content.Contains("Phone") && gte.Content.Contains("Email") => UserBadMultiple,
				422 when gte.Content.Contains("email") && gte.Content.Contains("password") => UserBadMultiple,
				422 when gte.Content.Contains("Phone") => UserBadPhoneNumber,
				422 when gte.Content.Contains("phone") => UserBadPhoneNumber,
				422 when gte.Content.Contains("Phone") => UserBadPhoneNumber,
				422 when gte.Content.Contains("phone") => UserBadPhoneNumber,
				422 when gte.Content.Contains("Email") => UserBadEmailAddress,
				422 when gte.Content.Contains("email") => UserBadEmailAddress,
				422 when gte.Content.Contains("Password") => UserBadPassword,
				422 when gte.Content.Contains("password") => UserBadPassword,
				422 when gte.Content.Contains("provide") => UserMissingInformation,
				429 => UserTooManyRequests,
				_ => Unknown
			};

		}
	}
}
