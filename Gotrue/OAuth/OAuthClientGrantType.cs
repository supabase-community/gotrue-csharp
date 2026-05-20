using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue.OAuth
{
    /// <summary>
    /// Represents the OAuth grant types supported by the client.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthClientGrantType
    {
        [EnumMember(Value = "authorization_code")]
        AuthorizationCode,

        [EnumMember(Value = "refresh_token")]
        RefreshToken,
    }
}
