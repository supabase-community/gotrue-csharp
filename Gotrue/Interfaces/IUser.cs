using System;
using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the action link.
        /// </summary>
        /// <value>
        /// The action link.
        /// </value>
        string ActionLink { get; set; }
        /// <summary>
        /// Gets or sets the application metadata.
        /// </summary>
        /// <value>
        /// The application metadata.
        /// </value>
        Dictionary<string, object> AppMetadata { get; set; }
        /// <summary>
        /// Gets or sets the aud.
        /// </summary>
        /// <value>
        /// The aud.
        /// </value>
        string Aud { get; set; }
        /// <summary>
        /// Gets or sets the confirmation sent at.
        /// </summary>
        /// <value>
        /// The confirmation sent at.
        /// </value>
        DateTime? ConfirmationSentAt { get; set; }
        /// <summary>
        /// Gets or sets the confirmed at.
        /// </summary>
        /// <value>
        /// The confirmed at.
        /// </value>
        DateTime? ConfirmedAt { get; set; }
        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        DateTime CreatedAt { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        string Email { get; set; }
        /// <summary>
        /// Gets or sets the email confirmed at.
        /// </summary>
        /// <value>
        /// The email confirmed at.
        /// </value>
        DateTime? EmailConfirmedAt { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; set; }
        /// <summary>
        /// Gets or sets the identities.
        /// </summary>
        /// <value>
        /// The identities.
        /// </value>
        List<UserIdentity> Identities { get; set; }
        /// <summary>
        /// Gets or sets the invited at.
        /// </summary>
        /// <value>
        /// The invited at.
        /// </value>
        DateTime? InvitedAt { get; set; }
        /// <summary>
        /// Gets or sets the last sign in at.
        /// </summary>
        /// <value>
        /// The last sign in at.
        /// </value>
        DateTime? LastSignInAt { get; set; }
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        string Phone { get; set; }
        /// <summary>
        /// Gets or sets the phone confirmed at.
        /// </summary>
        /// <value>
        /// The phone confirmed at.
        /// </value>
        DateTime? PhoneConfirmedAt { get; set; }
        /// <summary>
        /// Gets or sets the recovery sent at.
        /// </summary>
        /// <value>
        /// The recovery sent at.
        /// </value>
        DateTime? RecoverySentAt { get; set; }
        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        string Role { get; set; }
        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// Gets or sets the user metadata.
        /// </summary>
        /// <value>
        /// The user metadata.
        /// </value>
        Dictionary<string, object> UserMetadata { get; set; }
    }
}