using System.Collections.Generic;

namespace Aub.Eece503e.ChatService.DataContracts
{
    public class GetConversationsResponse
    {
        public List<ConversationsInfo> Conversations { get; set; }
        public string NextUri { get; set; }
    }
}