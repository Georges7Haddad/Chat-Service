using System;
using System.Net;

namespace Aub.Eece503e.ChatService.Client
{
    public class ChatServiceException: Exception
    {
        public HttpStatusCode StatusCode { get; }
        public ChatServiceException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}