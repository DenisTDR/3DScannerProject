using System;
using Newtonsoft.Json;

namespace TDR.ChatLibrary.Messages.Responses
{
    public class DataResponse : IMessage
    {
        [JsonProperty(PropertyName = "wh")]
        public string What { get; set; }

        [JsonProperty(PropertyName = "dt")]
        public object Data { get; set; }


        [JsonProperty(PropertyName = "tp")]
        public Type DataType { get; set; }
    }
}