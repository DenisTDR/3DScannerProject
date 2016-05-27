using Newtonsoft.Json;
using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages.Responses
{
    public class SuccessResponse : IMessageResponse
    {

        [JsonProperty(PropertyName = "tid")]
        public MessageId TargetId { get; set; }
    }
}
