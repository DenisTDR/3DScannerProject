using System;
using System.Collections.Generic;
using TDR.ChatLibrary.Messages;

namespace TDR.ChatLibrary.Client
{
    public interface IChatClient
    {
        bool IsServerSideClient { get; }
        bool IsLoggedIn { get; set; }
        string UserName { get; set; }
        bool IsConnected { get; }
        ChatClientConnectionState ConnectionState { get; }
        event ChatClientConnectionStateChangedEventHandler ConnectionStateChanged;
        event ChatClientMessageReceivedEventHandler MessageReceived;
        event ChatClientLostConnectionEventHandler LostConnection;
        event ChatClientDataResponseReceivedEventHandler DataResponseReceived;
        void Connect();
        void Disconnect();
        void SendMessage(IMessage message);
    }
}
