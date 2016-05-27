using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Messages.Responses
{
    public interface IMessageResponse : IMessage
    {
        MessageId TargetId { get; set; }
    }
}
