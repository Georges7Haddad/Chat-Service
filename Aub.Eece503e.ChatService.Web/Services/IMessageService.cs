using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public interface IMessageService
    {
        Task<Message> AddMessage(string conversationId, AddMessageRequestBody addMessageRequestBody);
        Task<GetMessagesResult> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime);
        Task<Message> GetMessage(string conversationId, string messageId);
        Task DeleteMessage(string conversationId, string messageId);
    }
}