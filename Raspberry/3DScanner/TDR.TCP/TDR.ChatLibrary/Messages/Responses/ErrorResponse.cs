using Newtonsoft.Json;
using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages.Responses
{
    public class ErrorResponse : IMessage
    {
        [JsonProperty(PropertyName = "tid")]
        public MessageId TargetId { get; set; }

        [JsonProperty(PropertyName = "rez")]
        public string Reason { get; set; }
    }
}
