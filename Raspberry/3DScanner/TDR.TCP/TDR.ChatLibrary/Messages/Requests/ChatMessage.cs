using Newtonsoft.Json;
using TDR.TCPDataBaseManagement.ViewModels;

namespace TDR.ChatLibrary.Messages.Requests
{
    public class ChatMessage : TextMessage
    {
        [JsonProperty(PropertyName = "s")]
        public UserViewModel Sender { get; set; }
        
        [JsonProperty(PropertyName = "r")]
        public UserViewModel Recipient { get; set; }
    }
}
