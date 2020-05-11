using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace Aub.Eece503e.ChatService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;

        public ConversationsController(IMessageService messageService, IConversationService conversationService)
        {
            _messageService = messageService;
            _conversationService = conversationService;
        }
        
        [HttpGet(
            "{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            var messagesResult =
                await _messageService.GetMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
            
            string nextUri = null;
            if (!string.IsNullOrWhiteSpace(messagesResult.ContinuationToken))
            {
                nextUri =
                    $"api/conversations/{conversationId}/messages?continuationToken={WebUtility.UrlEncode(messagesResult.ContinuationToken)}&limit={limit}&lastSeenMessageTime={lastSeenMessageTime}";
            }
            var messagesResponse = new GetMessagesResponse
            {
                NextUri = nextUri,
                Messages = messagesResult.Messages
            };
            return Ok(messagesResponse);
        }
        
        [HttpPost("{conversationId}/messages")]
        public async Task<IActionResult> AddMessage(string conversationId, [FromBody]AddMessageRequestBody addMessageRequestBody)
        {
            var message = await _messageService.AddMessage(conversationId, addMessageRequestBody);
            return StatusCode(201, message);
        }

        [HttpDelete("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(string conversationId, string messageId)
        {
            await _messageService.DeleteMessage(conversationId, messageId);
            return Ok();
        }
        
        [HttpGet("{conversationId}/messages/{messageId}")]
        public async Task<IActionResult> GetMessage(string conversationId, string messageId)
        { 
            var message = await _messageService.GetMessage(conversationId, messageId); 
            return Ok(message);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            var conversationsResult =
                await _conversationService.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
            
            string nextUri = null;
            if (!string.IsNullOrWhiteSpace(conversationsResult.ContinuationToken))
            {
                nextUri =
                    $"api/conversations?username={username}&continuationToken={WebUtility.UrlEncode(conversationsResult.ContinuationToken)}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}";
            }

            var conversationsResponse = new GetConversationsResponse
            {
                NextUri = nextUri,
                Conversations = conversationsResult.Conversations
            };
            
            return Ok(conversationsResponse);
        }

        [HttpPost]
        public async Task<IActionResult> AddConversation([FromBody]AddConversationRequestBody requestBody)
        {
            var addConversationResponse = await _conversationService.AddConversation(requestBody);
            return StatusCode(201, addConversationResponse);
        }

        [HttpDelete("{conversationId}/{username}")]
        public async Task<IActionResult> DeleteConversation(string username, string conversationId)
        {
            await _conversationService.DeleteConversation(username, conversationId);
            return Ok();
        }
        
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(string conversationId)
        {
            var conversation = await _conversationService.GetConversation(conversationId); 
            return Ok(conversation);
        }
    }
}