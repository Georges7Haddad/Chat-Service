using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class AddConversationResponse
    { 
       public string Id { get; set; }
       public long CreatedUnixTime { get; set; }
    }
}