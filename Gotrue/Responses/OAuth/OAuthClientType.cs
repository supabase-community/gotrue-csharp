using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue.Responses.OAuth
{
    /// <summary>
    /// Represents the type of OAuth client.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthClientType
    {
        [EnumMember(Value = "public")]
        Public,

        [EnumMember(Value = "confidential")]
        Confidential,
    }
}
