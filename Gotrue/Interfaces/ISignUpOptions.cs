using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface ISignUpOptions
    {
        /// <summary>
        /// A URL or mobile address to send the user to after they are confirmed.
        /// </summary>
        public string RedirectTo { get; set; }

        Dictionary<string, object> Data { get; set; }
    }
}