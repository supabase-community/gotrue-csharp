using System.Net.Http;

namespace Supabase.Gotrue.Exceptions
{
    public class UnauthorizedException : GotrueException
    {
        public HttpResponseMessage Response { get; private set; }

        public string? Content { get; private set; }
        public UnauthorizedException(RequestException exception) : base(exception.Error.Message, exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
