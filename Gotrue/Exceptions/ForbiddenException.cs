using System;
using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
    public class ForbiddenException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string? Content { get; private set; }
        public ForbiddenException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
