using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface ISignUpOptions
    {
        /// <summary>
        /// A URL or mobile address to send the user to after they are confirmed.
        /// </summary>
        /// <value>
        /// The redirect to.
        /// </value>
        public string RedirectTo { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        Dictionary<string, object> Data { get; set; }
    }
}