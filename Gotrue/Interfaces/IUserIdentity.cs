using System;
using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUserIdentity
    {
        DateTime CreatedAt { get; set; }
        string Id { get; set; }
        Dictionary<string, object> IdentityData { get; set; }
        DateTime LastSignInAt { get; set; }
        string Provider { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UserId { get; set; }
    }
}