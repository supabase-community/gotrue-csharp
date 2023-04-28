using System;
using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
    public class ExistingUserException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string? Content { get; private set; }
        public ExistingUserException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
