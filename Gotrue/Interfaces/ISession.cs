using System;

namespace Supabase.Gotrue.Interfaces
{
    public interface ISession
    {
        string AccessToken { get; set; }
        DateTime CreatedAt { get; }
        int ExpiresIn { get; set; }
        string RefreshToken { get; set; }
        string TokenType { get; set; }
        IUser User { get; set; }
        DateTime ExpiresAt();
    }
}