using System;
using System.Diagnostics;
using System.Net;
using Supabase.Gotrue.Exceptions;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Internal class for parsing Supabase specific exceptions.
    /// </summary>
    internal static class ExceptionHandler
    {
        internal static Exception Parse(RequestException ex)
        {
            switch (ex.Response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    Debug.WriteLine(ex.Message);
                    return new UnauthorizedException(ex);
                case HttpStatusCode.BadRequest:
                    Debug.WriteLine(ex.Message);
                    return new BadRequestException(ex);
                case HttpStatusCode.Forbidden:
                    Debug.WriteLine("Forbidden, are sign-ups disabled?");
                    return new ForbiddenException(ex);
            }
            return ex;
        }
    }
}
