using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class MessagesInfo
    {
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public long UnixTime { get; set; }
        
        public override bool Equals(object obj)
        {
            return obj is MessagesInfo messagesInfo &&
                   Text == messagesInfo.Text &&
                   SenderUsername == messagesInfo.SenderUsername;
        }
        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SenderUsername);
            return hashCode;
        }
    }
}