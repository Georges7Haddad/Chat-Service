using System;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class ProfilePictureNotFoundException:Exception
    {
        public ProfilePictureNotFoundException(string message) : base(message)
        {
        }
    }
}