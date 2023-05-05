using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
    public class BadRequestException : GotrueException
    {
        public HttpResponseMessage Response { get; private set; }

        public string? Content { get; private set; }
        public BadRequestException(RequestException exception): base(exception.Error.Message, exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
