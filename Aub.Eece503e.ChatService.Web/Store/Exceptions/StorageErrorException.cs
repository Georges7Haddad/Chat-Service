using System;
using System.Net;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class StorageErrorException:Exception
    {
        public StorageErrorException()
        {
        }

        public StorageErrorException(string message, Exception innerexception) : base(message, innerexception)
        {
        }
    }
}