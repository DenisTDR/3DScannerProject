using Newtonsoft.Json;

namespace TDR.ChatLibrary.Messages.Requests
{
    public class DataRequest : IMessage
    {

        [JsonProperty(PropertyName = "wh")]
        public string What { get; set; }

        [JsonProperty(PropertyName = "ai")]
        public string AuxInfo { get; set; }
    }
}
