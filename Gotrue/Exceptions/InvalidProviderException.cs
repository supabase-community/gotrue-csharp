using System;
namespace Supabase.Gotrue.Exceptions
{
    public class InvalidProviderException : Exception
    {
        public InvalidProviderException(string message) : base(message) { }
    }
}

