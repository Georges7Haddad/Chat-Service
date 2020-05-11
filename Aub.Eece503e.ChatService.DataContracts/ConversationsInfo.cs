using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class ConversationsInfo
    {
        public string Id { get; set; }
        public long LastModifiedUnixTime { get; set; }
        public UserProfile Recipient { get; set; }
        
        public override bool Equals(object obj)
        {
            return obj is ConversationsInfo conversationsInfo &&
                   Id == conversationsInfo.Id &&
                   Recipient.Username == conversationsInfo.Recipient.Username &&
                   Recipient.FirstName == conversationsInfo.Recipient.FirstName &&
                   Recipient.LastName == conversationsInfo.Recipient.LastName &&
                   Recipient.ProfilePictureId == conversationsInfo.Recipient.ProfilePictureId;
        }
        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<UserProfile>.Default.GetHashCode(Recipient);
            return hashCode;
        }
    }
}