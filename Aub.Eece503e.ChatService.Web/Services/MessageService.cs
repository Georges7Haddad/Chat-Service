using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Exceptions;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageStore _messageStore;
        private readonly IConversationStore _conversationStore;
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;
        
        public MessageService(ILogger<ConversationsController> logger, IMessageStore messageStore, TelemetryClient telemetryClient, IConversationStore conversationStore)
        {
            _logger = logger;
            _messageStore = messageStore;
            _telemetryClient = telemetryClient;
            _conversationStore = conversationStore;
        }

        public async Task<Message> AddMessage(string conversationId, AddMessageRequestBody addMessageRequestBody)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var message = new Message
                {
                    Id = addMessageRequestBody.Id,
                    SenderUsername = addMessageRequestBody.SenderUsername,
                    Text = addMessageRequestBody.Text,
                    UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                ThrowBadRequestIfMessageInvalid(message);
                try
                {
                    var stopWatch = Stopwatch.StartNew();
                    await _messageStore.AddMessage(conversationId, message);
                    _telemetryClient.TrackMetric("MessageStore.AddMessage.Time", stopWatch.ElapsedMilliseconds);
                    _telemetryClient.TrackEvent("MessageCreated");
                }
                catch (MessageAlreadyExistsException e)
                {
                    message = await _messageStore.GetMessage(conversationId, message.Id);
                    return message;
                }

                await _conversationStore.UpdateConversation(message.UnixTime, conversationId);
                return message;
            }
        }

        public async Task<GetMessagesResult> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew(); 
                var messagesResult = await _messageStore.GetMessages(conversationId, continuationToken, limit, lastSeenMessageTime);
                _telemetryClient.TrackMetric("MessageStore.GetMessages.Time", stopWatch.ElapsedMilliseconds);

                return messagesResult;
            }
        }

        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew();
                var message = await _messageStore.GetMessage(conversationId, messageId);
                _telemetryClient.TrackMetric("MessageStore.GetMessage.Time", stopWatch.ElapsedMilliseconds);
                return message;
            }
        }

        public async Task DeleteMessage(string conversationId, string messageId)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew();
                await _messageStore.DeleteMessage(conversationId, messageId);

                _telemetryClient.TrackMetric("MessageStore.DeleteMessage.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("MessageDeleted");
            }
        }

        private void ThrowBadRequestIfMessageInvalid(Message message)
        {
            string error = null;
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                error = "The text must not be empty";
            }

            if (error != null)
            {
                throw new BadRequestException(error);
            }
        }
    }
}