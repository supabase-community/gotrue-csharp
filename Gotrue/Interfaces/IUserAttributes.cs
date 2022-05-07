using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUserAttributes
    {
        Dictionary<string, object> Data { get; set; }
        string Email { get; set; }
        string EmailChangeToken { get; set; }
        string Password { get; set; }
        string Phone { get; set; }
    }
}