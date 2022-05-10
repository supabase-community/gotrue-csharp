using System;
using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUserIdentity
    {
        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        DateTime CreatedAt { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; set; }
        /// <summary>
        /// Gets or sets the identity data.
        /// </summary>
        /// <value>
        /// The identity data.
        /// </value>
        Dictionary<string, object> IdentityData { get; set; }
        /// <summary>
        /// Gets or sets the last sign in at.
        /// </summary>
        /// <value>
        /// The last sign in at.
        /// </value>
        DateTime LastSignInAt { get; set; }
        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        string Provider { get; set; }
        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        string UserId { get; set; }
    }
}