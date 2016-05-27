using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.DataAccess;
using TDR.TCPDataBaseManagement.Models;
using TDR.TCPDataBaseManagement.ViewModels;

namespace TDR.TCPDataBaseManagement
{
    public class ChatDbRepository : IChatRepository
    {
        public bool SaveTextMessage(ChatMessageViewModel chatMessage)
        {
            using (var db = new DbUnit())
            {
                var messageModel = new ChatMessageModel
                {
                    Sender = db.Repository<UserModel>().Find(user => user.Username.Equals(chatMessage.Sender.Username)),
                    Recipient =
                        db.Repository<UserModel>().Find(user => user.Username.Equals(chatMessage.Recipient.Username)),
                    Message = chatMessage.Message
                };
                if (messageModel.Sender == null || messageModel.Recipient == null || messageModel.Message == null)
                {
                    return false;
                }
                chatMessage.Sender.Nick = messageModel.Sender.Nick;
                db.Repository<ChatMessageModel>().AddAsync(messageModel);
                return true;
            }
        }
    }
}
