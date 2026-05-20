using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Supabase.Gotrue.Responses.OAuth
{
    /// <summary>
    /// Represents the registration type of the OAuth client.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OAuthClientRegistrationType
    {
        [EnumMember(Value = "dynamic")]
        Dynamic,

        [EnumMember(Value = "manual")]
        Manual,
    }
}
