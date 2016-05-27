using Newtonsoft.Json;

namespace TDR.ChatLibrary.Messages.Responses
{
    public class AuthResponse : IMessage
    {
        [JsonProperty(PropertyName = "o")]
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "i")]
        public string Info { get; set; }
    }
}