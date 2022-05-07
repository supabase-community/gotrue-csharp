using System;
using System.Collections.Generic;

namespace Supabase.Gotrue.Interfaces
{
    public interface IUser
    {
        string ActionLink { get; set; }
        Dictionary<string, object> AppMetadata { get; set; }
        string Aud { get; set; }
        DateTime? ConfirmationSentAt { get; set; }
        DateTime? ConfirmedAt { get; set; }
        DateTime CreatedAt { get; set; }
        string Email { get; set; }
        DateTime? EmailConfirmedAt { get; set; }
        string Id { get; set; }
        List<UserIdentity> Identities { get; set; }
        DateTime? InvitedAt { get; set; }
        DateTime? LastSignInAt { get; set; }
        string Phone { get; set; }
        DateTime? PhoneConfirmedAt { get; set; }
        DateTime? RecoverySentAt { get; set; }
        string Role { get; set; }
        DateTime? UpdatedAt { get; set; }
        Dictionary<string, object> UserMetadata { get; set; }
    }
}