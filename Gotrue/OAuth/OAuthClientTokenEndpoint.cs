using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue.OAuth
{
    /// <summary>
    /// Represents the token endpoint authentication methods for OAuth clients.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthClientTokenEndpoint
    {
        [EnumMember(Value = "none")]
        None,

        [EnumMember(Value = "client_secret_basic")]
        ClientSecretBasic,

        [EnumMember(Value = "client_secret_post")]
        ClientSecretPost,
    }
}
