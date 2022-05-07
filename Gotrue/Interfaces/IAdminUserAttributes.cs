using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IAdminUserAttributes : IUserAttributes
    {
        Dictionary<string, object> AppMetadata { get; set; }
        bool? EmailConfirm { get; set; }
        bool? PhoneConfirm { get; set; }
        Dictionary<string, object> UserMetadata { get; set; }
    }
}