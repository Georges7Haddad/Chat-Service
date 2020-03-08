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
    }
    
}