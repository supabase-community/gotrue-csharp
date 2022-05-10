using System.Net.Http;

namespace Supabase.Gotrue.Interfaces
{
    public interface IBaseResponse
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        string Content { get; set; }
        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        HttpResponseMessage ResponseMessage { get; set; }
    }
}