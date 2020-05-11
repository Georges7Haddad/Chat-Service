
using System;
using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public long UnixTime { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Message message &&
                   Id == message.Id &&
                   Text == message.Text &&
                   SenderUsername == message.SenderUsername;
        }
        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SenderUsername);
            hashCode = hashCode * -1521134295 + UnixTime.GetHashCode();
            return hashCode;
        }
    }
}