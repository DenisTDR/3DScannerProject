using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.ChatLibrary.Client;
using TDR.ChatLibrary.Messages;
using TDR.ChatLibrary.Messages.Requests;
using TDR.ChatLibrary.Messages.Responses;
using TDR.TCPDataBaseManagement;
using TDR.TCPDataBaseManagement.DataAccess;
using IMessage = System.Runtime.Remoting.Messaging.IMessage;

namespace TDR.ChatLibrary
{
    public class DataRequestResolver
    {
        private IUserRepository _userRepository;

        public DataRequestResolver(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }

        public DataResponse ProcessDataRequestMessage(DataRequest request, IChatClient sender)
        {
            var dr = new DataResponse();
            switch (request.What)
            {
                case "users":
                    dr.Data = _userRepository.GetUsers().Where(user => user.Username != sender.UserName).ToList();
                    break;
                case "myInfo":
                    dr.Data = _userRepository.GetUser(sender.UserName);
                    break;
                default:
                    dr.Data = "nada";
                    break;
            }
            dr.What = request.What;
            dr.DataType = dr.Data.GetType();
            return dr;
        }
    }
}
