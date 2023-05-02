using System;
using System.Net.Http;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Responses;

namespace Supabase.Gotrue
{
    public class RequestException : GotrueException
    {
        public HttpResponseMessage Response { get; private set; }
        public ErrorResponse Error { get; private set; }

        public RequestException(HttpResponseMessage response, ErrorResponse error) : base(error.Message)
        {
            Response = response;
            Error = error;
        }
    }
}
