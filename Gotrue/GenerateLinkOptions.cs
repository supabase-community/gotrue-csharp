using System.Collections.Generic;
using Newtonsoft.Json;
using Supabase.Core.Attributes;

namespace Supabase.Gotrue
{
	/// <summary>
	/// Options for Generating an Email Link
	/// </summary>
	public class GenerateLinkOptions
	{
		/// <summary>
		/// Mapping of link types that can be generated.
		/// </summary>
		public enum LinkType
		{
			[MapTo("signup")]
			SignUp,
			[MapTo("invite")]
			Invite,
			[MapTo("magiclink")]
			MagicLink,
			[MapTo("recovery")]
			Recovery,
			[MapTo("email_change_current")]
			EmailChangeCurrent,
			[MapTo("email_change_new")]
			EmailChangeNew
		}
		
		/// <summary>
		/// The type of link being generated
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; }
		
		/// <summary>
		/// The User's Email
		/// </summary>
		[JsonProperty("email")]
		public string Email { get; }
		
		/// <summary>
		/// Only required if generating a signup link.
		/// </summary>
		[JsonProperty("password")]
		public string? Password { get; set; }
		
		/// <summary>
		/// The user's new email. Only required if type is 'email_change_current' or 'email_change_new'.
		/// </summary>
		[JsonProperty("new_email")]
		public string? NewEmail { get; set; }
		
		/// <summary>
		/// A custom data object to store the user's metadata. This maps to the `auth.users.user_metadata` column.
		///
		/// The `data` should be a JSON encodable object that includes user-specific info, such as their first and last name.
		/// </summary>
		[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, object>? Data { get; set; }
		
		/// <summary>
		/// The URL which will be appended to the email link generated.
		/// </summary>
		public string? RedirectTo { get; set; }

		/// <summary>
		/// Constructs options, additional properties may need to be assigned depending on <see cref="LinkType"/>
		///
		/// - <see cref="NewEmail"/> is required for <see cref="LinkType.EmailChangeCurrent"/> and <see cref="LinkType.EmailChangeNew"/>
		/// - <see cref="Password"/> is required for <see cref="LinkType.SignUp"/>
		/// - <see cref="Data"/> is optional for <see cref="LinkType.SignUp"/>
		/// </summary>
		/// <param name="linkType"></param>
		/// <param name="email"></param>
		public GenerateLinkOptions(LinkType linkType, string email)
		{
			Type = Core.Helpers.GetMappedToAttr(linkType).Mapping;
			Email = email;
		}
	}

	/// <summary>
	/// Shortcut options for <see cref="GenerateLinkOptions.LinkType.SignUp"/>
	/// </summary>
	public class GenerateLinkSignupOptions : GenerateLinkOptions
	{
		public GenerateLinkSignupOptions(string email, string password) : base(LinkType.SignUp, email)
		{
			Password = password;
		}
	}
	
	/// <summary>
	/// Shortcut options for <see cref="GenerateLinkOptions.LinkType.EmailChangeCurrent"/>
	/// </summary>
	public class GenerateLinkEmailChangeCurrentOptions: GenerateLinkOptions
	{
		public GenerateLinkEmailChangeCurrentOptions(string email, string newEmail) : base(LinkType.EmailChangeCurrent, email)
		{
			NewEmail = newEmail;
		}
	}
	
	/// <summary>
	/// Shortcut options for <see cref="GenerateLinkOptions.LinkType.EmailChangeNew"/>
	/// </summary>
	public class GenerateLinkEmailChangeNewOptions: GenerateLinkOptions
	{
		public GenerateLinkEmailChangeNewOptions(string email, string newEmail) : base(LinkType.EmailChangeNew, email)
		{
			NewEmail = newEmail;
		}
	}
}