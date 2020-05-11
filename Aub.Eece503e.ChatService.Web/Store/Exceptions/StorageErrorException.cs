using System;
using System.Net;

namespace Aub.Eece503e.ChatService.Web.Store.Exceptions
{
    public class StorageErrorException:Exception
    {
        public int StatusCode { get; }
        public StorageErrorException()
        {
        }

        public StorageErrorException(string message, Exception innerException, int statusCode) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
        public StorageErrorException(string message, Exception innerexception) : base(message, innerexception)
        {
        }
        public StorageErrorException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}