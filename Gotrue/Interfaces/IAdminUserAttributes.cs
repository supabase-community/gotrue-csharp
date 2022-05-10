using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    /// <seealso cref="Supabase.Gotrue.Interfaces.IUserAttributes" />
    public interface IAdminUserAttributes : IUserAttributes
    {
        /// <summary>
        /// Gets or sets the application metadata.
        /// </summary>
        /// <value>
        /// The application metadata.
        /// </value>
        Dictionary<string, object> AppMetadata { get; set; }
        /// <summary>
        /// Gets or sets the email confirm.
        /// </summary>
        /// <value>
        /// The email confirm.
        /// </value>
        bool? EmailConfirm { get; set; }
        /// <summary>
        /// Gets or sets the phone confirm.
        /// </summary>
        /// <value>
        /// The phone confirm.
        /// </value>
        bool? PhoneConfirm { get; set; }
        /// <summary>
        /// Gets or sets the user metadata.
        /// </summary>
        /// <value>
        /// The user metadata.
        /// </value>
        Dictionary<string, object> UserMetadata { get; set; }
    }
}