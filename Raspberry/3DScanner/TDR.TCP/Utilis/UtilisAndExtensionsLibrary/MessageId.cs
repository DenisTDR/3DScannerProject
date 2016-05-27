using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilisAndExtensionsLibrary
{
    public class MessageId
    {
        public string Id { get; set; }

        public static MessageId NewMessageId()
        {
            return new MessageId
            {
                Id = Guid.NewGuid().ToString().Substring(24)
            };
        }

        public override bool Equals(object o)
        {
            return o is MessageId && Equals((MessageId) o);
        }

        protected bool Equals(MessageId other)
        {
            return string.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
