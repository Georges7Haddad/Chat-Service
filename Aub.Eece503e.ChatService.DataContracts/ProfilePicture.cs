namespace Aub.Eece503e.ChatService.DataContracts
{
    public class ProfilePicture
    {
        public string ProfilePictureId { get; set; }
        public byte[] Image { get; set; }
        
        public override bool Equals(object obj)
        {
            return obj is ProfilePicture profilePicture &&
                   Image == profilePicture.Image &&
                   ProfilePictureId == profilePicture.ProfilePictureId;
        }
    }
}