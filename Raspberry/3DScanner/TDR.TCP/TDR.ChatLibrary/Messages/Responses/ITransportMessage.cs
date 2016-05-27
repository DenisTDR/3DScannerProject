using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages.Responses
{
    public interface ITransportMessage : IMessage
    {
        MessageId Id { get; set; }
    }
}
