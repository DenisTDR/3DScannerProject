using Newtonsoft.Json;
using TDR.ChatLibrary.Messages.Responses;
using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages.Requests
{
    public class TextMessage : ITransportMessage
    {
        public MessageId Id { get; set; }
        [JsonProperty(PropertyName = "mb")]
        public string MessageBody { get; set; }
    }
}
