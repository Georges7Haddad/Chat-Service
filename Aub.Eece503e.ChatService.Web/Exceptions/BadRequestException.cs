using System;

namespace Aub.Eece503e.ChatService.Web.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}