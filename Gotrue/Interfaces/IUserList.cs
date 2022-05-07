using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUserList
    {
        string Aud { get; set; }
        List<User> Users { get; set; }
    }
}