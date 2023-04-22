using System;
using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public string? Content { get; private set; }
        public UnauthorizedException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
