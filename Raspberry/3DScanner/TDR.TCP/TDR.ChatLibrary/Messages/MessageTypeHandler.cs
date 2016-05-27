using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.ChatLibrary.Messages.Others;
using TDR.ChatLibrary.Messages.Requests;
using TDR.ChatLibrary.Messages.Responses;

namespace TDR.ChatLibrary.Messages
{
    public static class MessageTypeHandler
    {
        public static Type GetTypeByName(string typeName)
        {
            switch (typeName)
            {
                case "AuthRequest":
                    return typeof(AuthRequest);
                case "AuthRequired":
                    return typeof(AuthRequired);
                case "AuthResponse":
                    return typeof(AuthResponse);
                case "BruteMessage":
                    return typeof(BruteMessage);
                case "ChatMessage":
                    return typeof(ChatMessage);
                case "DataRequest":
                    return typeof(DataRequest);
                case "DataResponse":
                    return typeof(DataResponse);
                case "ErrorResponse":
                    return typeof(ErrorResponse);
                case "KeepAlive":
                    return typeof(KeepAlive);
                case "TextMessage":
                    return typeof(TextMessage);
                case "Welcome":
                    return typeof(Welcome);
                default:
                    return typeof (IMessage);
            }
        }
        public static Type GetTypeByName(MessageType typeName)
        {
            switch (typeName)
            {
                case MessageType.AuthRequest:
                    return typeof(AuthRequest);
                case MessageType.AuthRequired:
                    return typeof(AuthRequired);
                case MessageType.AuthResponse:
                    return typeof(AuthResponse);
                case MessageType.ChatMessage:
                    return typeof(ChatMessage);
                case MessageType.DataRequest:
                    return typeof(DataRequest);
                case MessageType.DataResponse:
                    return typeof(DataResponse);
                case MessageType.ErrorResponse:
                    return typeof(ErrorResponse);
                case MessageType.KeepAlive:
                    return typeof(KeepAlive);
                case MessageType.TextMessage:
                    return typeof(TextMessage);
                case MessageType.Welcome:
                    return typeof(Welcome);
                default:
                    return typeof(IMessage);
            }
        }
    }
}
