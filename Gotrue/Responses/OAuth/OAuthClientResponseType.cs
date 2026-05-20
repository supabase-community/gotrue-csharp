using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue.Responses.OAuth
{
    /// <summary>
    /// Represents the OAuth response types supported by the client.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthClientResponseType
    {
        [EnumMember(Value = "code")]
        Code,
    }
}
