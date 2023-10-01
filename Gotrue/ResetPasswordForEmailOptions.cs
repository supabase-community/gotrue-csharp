namespace Supabase.Gotrue
{
	/// <summary>
	/// A utility class that represents a successful response from a request to send a user's password reset using the PKCE flow.
	/// </summary>
	public class ResetPasswordForEmailOptions
	{
		/// <summary>
		/// The Email representing the user's account whose password is being reset.
		/// </summary>
		public string Email { get; private set; }

		/// <summary>
		/// The OAuth Flow Type. 
		/// </summary>
		public Constants.OAuthFlowType FlowType { get; set; } = Constants.OAuthFlowType.Implicit;

		/// <summary>
		/// The URL to send the user to after they click the password reset link.
		/// </summary>
		public string? RedirectTo { get; set; }

		/// <summary>
		/// Verification token received when the user completes the captcha on the site.
		/// </summary>
		public string? CaptchaToken { get; set; }

		/// <summary>
		/// PKCE Verifier generated if using the PKCE flow type.
		/// </summary>
		public string? PKCEVerifier { get; set; }

		public ResetPasswordForEmailOptions(string email)
		{
			Email = email;
		}
	}
}