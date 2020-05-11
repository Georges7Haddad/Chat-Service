using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Controllers;
using Aub.Eece503e.ChatService.Web.Store;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace Aub.Eece503e.ChatService.Web.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IProfileStore _profileStore;
        private readonly ILogger<ConversationsController> _logger;
        private readonly TelemetryClient _telemetryClient;
        
        public ConversationService(ILogger<ConversationsController> logger, IConversationStore conversationStore, TelemetryClient telemetryClient, IProfileStore profileStore, IMessageStore messageStore)
        {
            _logger = logger;
            _conversationStore = conversationStore;
            _telemetryClient = telemetryClient;
            _profileStore = profileStore;
            _messageStore = messageStore;
        }

        public async Task<AddConversationResponse> AddConversation(AddConversationRequestBody addConversationRequestBody)
        {
            var id = "m_" + addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1];
            var conversation = new Conversation
            {
                Id = id,
                Participants = addConversationRequestBody.Participants,
                LastModifiedUnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            var addConversationResponse = new AddConversationResponse
            {
                Id = addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1],
                CreatedUnixTime = conversation.LastModifiedUnixTime
            };
            
            using (_logger.BeginScope("{ConversationId}", id))
            {
                var stopWatch = Stopwatch.StartNew();
                await _conversationStore.AddConversation(conversation);
                _telemetryClient.TrackMetric("ConversationStore.AddConversation.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ConversationCreated");
                var message = new Message
                {
                    Id = addConversationRequestBody.FirstMessage["Id"],
                    Text = addConversationRequestBody.FirstMessage["Text"],
                    SenderUsername = addConversationRequestBody.FirstMessage["SenderUsername"],
                    UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                await _messageStore.AddMessage(id.Remove(0, 2), message);
                return addConversationResponse;
            }
        }

        public async Task<GetConversationsResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                var stopWatch = Stopwatch.StartNew(); 
                var conversationsResult = await _conversationStore.GetConversations(username, continuationToken, limit, lastSeenConversationTime);
                _telemetryClient.TrackMetric("ConversationStore.GetConversations.Time", stopWatch.ElapsedMilliseconds);

                foreach (var conversation in conversationsResult.Conversations)
                {
                    string[] usernames = conversation.Id.Split('_');
                    if (usernames[0] != username)
                    {
                        conversation.Recipient = await _profileStore.GetProfile(usernames[0]);
                    }
                    else
                    {
                        conversation.Recipient = await _profileStore.GetProfile(usernames[1]);
                    }
                }
                return conversationsResult;
            }
        }

        public async Task<Conversation> GetConversation(string conversationId)
        {
            string[] usernames = conversationId.Split('_');
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew();
                var conversation = await _conversationStore.GetConversation(usernames[1], conversationId);
                
                _telemetryClient.TrackMetric("ConversationStore.GetConversation.Time", stopWatch.ElapsedMilliseconds);
                return conversation;
            }
        }

        public async Task DeleteConversation(string username, string conversationId)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                var stopWatch = Stopwatch.StartNew();
                await _conversationStore.DeleteConversation(username, conversationId);

                _telemetryClient.TrackMetric("ConversationStore.DeleteConversation.Time", stopWatch.ElapsedMilliseconds);
                _telemetryClient.TrackEvent("ConversationDeleted");
            }
        }
    }
}