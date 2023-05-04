using System;
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
					return new UnauthorizedException(ex);
				case HttpStatusCode.BadRequest:
					return new BadRequestException(ex);
				case HttpStatusCode.Forbidden:
					return new ForbiddenException(ex);
			}
			return ex;
		}
	}
}
