using Newtonsoft.Json;

namespace TDR.ChatLibrary.Messages.Requests
{
    public class AuthRequest:IMessage
    {
        [JsonProperty(PropertyName = "pw")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "un")]
        public string Username { get; set; }
    }
}
