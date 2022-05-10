using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUserList
    {
        /// <summary>
        /// Gets or sets the aud.
        /// </summary>
        /// <value>
        /// The aud.
        /// </value>
        string Aud { get; set; }
        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        List<User> Users { get; set; }
    }
}