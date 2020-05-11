using System.Collections.Generic;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public class GetConversationsResult
    {
        public string ContinuationToken { get; set; }
        public List<ConversationsInfo> Conversations { get; set; }
    }
}