using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages
{
    public class BruteMessage : GenericBruteMessage<IMessage>
    {
        [JsonProperty(PropertyName = "mt")]
        public MessageType MessageType { get; set; }

        

        public BruteMessage(IMessage message) : base(message)
        {
            this.Message = message;
            this.MessageType = (MessageType) Enum.Parse(MessageType.GetType(), message.GetType().Name);
        }

        public static bool TryDebinarize(byte[] binary, out BruteMessage bruteMessage,
            out MessageDeserializingErrorInfo errorInfo)
        {
            bruteMessage = null;
            errorInfo = null;
            try
            {
                var jsonized = Serialize.BinaryToJson(binary);
                errorInfo = new MessageDeserializingErrorInfo {Json = jsonized};

                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonized);
                int typeIndex = 0;
                var exists = dict.Any(x => int.TryParse(x.Value.ToString(), out typeIndex));
                if (!exists)
                {
                    errorInfo = new MessageDeserializingErrorInfo
                    {
                        Json = JsonConvert.SerializeObject("Can't find messageType")
                    };
                    return false;
                }
                var messageTypeEnum = (MessageType) typeIndex;
                var messageType = MessageTypeHandler.GetTypeByName(messageTypeEnum);

                var type1 = typeof (GenericBruteMessage<>);
                var type2 = type1.MakeGenericType(messageType);

                dynamic obj = JsonConvert.DeserializeObject(jsonized, type2);
                if (!typeof (IMessage).IsAssignableFrom(messageType))
                {
                    return false;
                }
                bruteMessage = new BruteMessage(obj.Message);
                return true;
            }
            catch (Exception exc)
            {
                if (errorInfo == null)
                {
                    errorInfo = new MessageDeserializingErrorInfo {Exception = exc};
                }
                else
                {
                    errorInfo.Exception = exc;
                }
            }
            return false;
        }

       
    }
}
