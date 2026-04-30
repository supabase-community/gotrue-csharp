using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomProviderType
    {
        [EnumMember(Value = "oauth2")]
        OAuth2,

        [EnumMember(Value = "oidc")]
        Oidc,
    }
}
