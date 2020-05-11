using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class GetMessagesResponse
    {
        public List<MessagesInfo> Messages { get; set; }
        public string NextUri { get; set; }
    }
}