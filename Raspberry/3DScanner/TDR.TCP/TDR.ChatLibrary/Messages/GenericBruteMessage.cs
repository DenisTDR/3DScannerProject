using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TDR.ChatLibrary.Messages
{
    public class GenericBruteMessage<T> where T : IMessage
    {
        [JsonProperty(PropertyName = "msg")]
        public T Message { get; set; }

        public GenericBruteMessage(T message)
        {
            Message = message;
        }
    }
}
