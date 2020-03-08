using System;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class StorageConflictException:Exception
    {
        public StorageConflictException(string message, Exception exception) : base(message, exception)
        {
            
        }
    }
}