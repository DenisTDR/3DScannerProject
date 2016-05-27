using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.ChatLibrary.Messages
{
    public enum MessageType
    {
        KeepAlive,
        TextMessage,
        AuthRequest,
        AuthRequired,
        AuthResponse,
        Welcome,
        DataRequest,
        DataResponse,
        ChatMessage,
        ErrorResponse,
        SuccessResponse
    }
}
