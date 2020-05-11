using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class AddConversationRequestBody
    { 
        public List<string> Participants { get; set; }
        public Dictionary<string, string> FirstMessage { get; set; }
    }
}