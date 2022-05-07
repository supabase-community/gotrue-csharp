using System;
using System.Net.Http;
using Newtonsoft.Json;
using Supabase.Gotrue.Interfaces;

namespace Supabase.Gotrue.Responses
{
    /// <summary>
    /// A representation of Postgrest's API error response.
    /// </summary>
    public class ErrorResponse : IBaseResponse
    {
        public string Message { get; set; }
        public string Content { get; set; }
        public HttpResponseMessage ResponseMessage { get; set; }
    }
}
