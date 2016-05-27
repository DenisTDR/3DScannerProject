using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UtilisAndExtensionsLibrary
{
    public static class Serialize
    {
        public static byte[] ToBinary<T>(this T bruteMessage)
        {
            return Encoding.UTF8.GetBytes(bruteMessage.Jsonize());
        }

        public static string Jsonize<T>(this T bruteMessage)
        {
            return JsonConvert.SerializeObject(bruteMessage);
        }

        public static string BinaryToJson(byte[] binary)
        {
            return Encoding.UTF8.GetString(binary);
        }

        public static JsonSerializerSettings IgnoringExceptionJsonSerializerSettings()
        {
            return new JsonSerializerSettings {Error = (sender, e) =>
            {
                var currentError = e.ErrorContext.Error.Message;
                e.ErrorContext.Handled = true;
            }
            };
        }
    }
}
