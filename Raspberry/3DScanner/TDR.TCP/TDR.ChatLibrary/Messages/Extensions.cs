using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.ChatLibrary.Messages
{
    public static class Extensions
    {
        public static T ConvertTo<T>(this IMessage message) where T : IMessage
        {
            if (message is T)
                return (T)message;
            else
            {
                return default(T);
            }
        }
    }
}
