using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class UserProfile
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureId { get; set; }
        

        public override bool Equals(object obj)
        {
            return obj is UserProfile profile &&
                   Username == profile.Username &&
                   FirstName == profile.FirstName &&
                   LastName == profile.LastName && 
                   ProfilePictureId == profile.ProfilePictureId;
        }
        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Username);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProfilePictureId);
            return hashCode;
        }
    }
    
}