using System.Collections.Generic;
using Aub.Eece503e.ChatService.DataContracts;

namespace Aub.Eece503e.ChatService.Web.Store
{
    public class GetMessagesResult
    { 
        public string ContinuationToken { get; set; }
        public List<MessagesInfo> Messages { get; set; }
    }
}