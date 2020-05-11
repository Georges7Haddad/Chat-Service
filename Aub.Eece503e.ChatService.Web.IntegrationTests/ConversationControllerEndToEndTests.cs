using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Xunit;
namespace Aub.Eece503e.ChatService.Web.IntegrationTests
{
    public abstract class ConversationControllerEndtoEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture
    {
        private static readonly string _username = Guid.NewGuid().ToString();
        private static readonly string _username2 = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<string, string> _messagesToCleanup = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<List<string>, string> _conversationsToCleanup = new ConcurrentDictionary<List<string>, string>();
        private readonly ConcurrentBag<UserProfile> _usersToCleanup = new ConcurrentBag<UserProfile>();
        private IChatServiceClient _chatServiceClient { get; }

        public ConversationControllerEndtoEndTests(TFixture tfixture)
        {
            _chatServiceClient = tfixture.ChatServiceClient;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        
        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();
            foreach(var entry in _conversationsToCleanup)
            {
                var task = _chatServiceClient.DeleteConversation(entry.Key[0], entry.Value);
                var task2 = _chatServiceClient.DeleteConversation(entry.Key[1], entry.Value);
                tasks.Add(task);
                tasks.Add(task2);
            }
            foreach(KeyValuePair<string, string> entry in _messagesToCleanup)
            {
                var task = _chatServiceClient.DeleteMessage(entry.Key, entry.Value);
                tasks.Add(task);
            }
            foreach(var profile in _usersToCleanup)
            {
                var task = _chatServiceClient.DeleteProfile(profile.Username);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        
        [Fact]
        public async Task PostGetConversation()
        {
            var addConversationRequestBody = CreateRandomAddRequestBody(Guid.NewGuid().ToString());
            await _chatServiceClient.AddConversation(addConversationRequestBody);

            var id =  addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1];

            Conversation fetchedConversationFromSender = await _chatServiceClient.GetConversation(id);
            Conversation fetchedConversationFromReceiver = await _chatServiceClient.GetConversation(id);
            Message fetchedMessage = await _chatServiceClient.GetMessage(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Conversation conversation = new Conversation
            {
                Id = "m_" + id,
                Participants = new List<string> {  addConversationRequestBody.Participants[0], addConversationRequestBody.Participants[1]},
                LastModifiedUnixTime = fetchedConversationFromReceiver.LastModifiedUnixTime
            };
            _conversationsToCleanup.TryAdd(conversation.Participants, conversation.Id.Remove(0,2));
            
            Message message = new Message
            {
                Id = addConversationRequestBody.FirstMessage["Id"],
                Text = "ae",
                SenderUsername = addConversationRequestBody.Participants[0],
                UnixTime = fetchedMessage.UnixTime
            };
            _messagesToCleanup.TryAdd(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Assert.Equal(message, fetchedMessage);
            Assert.Equal(conversation,fetchedConversationFromReceiver);
            Assert.Equal(conversation,fetchedConversationFromSender);
        }
        
        [Fact]
        public async Task DeleteConversation()
        {
            var addConversationRequestBody = CreateRandomAddRequestBody(Guid.NewGuid().ToString());
            var id =  addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1];
            
            await _chatServiceClient.AddConversation(addConversationRequestBody);
            _messagesToCleanup.TryAdd(id, addConversationRequestBody.FirstMessage["Id"]);
            await _chatServiceClient.DeleteConversation(addConversationRequestBody.Participants[0], id);
            await _chatServiceClient.DeleteConversation(addConversationRequestBody.Participants[1], id);
        
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.GetConversation(id));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.GetConversation(id));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }
        
        [Fact]
        public async Task GetNewConversations()
        {
            var conversationsInfoList = new List<ConversationsInfo>();
            for (var i = 0; i < 4; ++i)
            {
                var body = CreateRandomAddRequestBody(_username);
                var conversationId =  body.Participants[0] + "_" + body.Participants[1];
                var profile = CreateAndAddUserProfile(body.Participants[1]);
                await _chatServiceClient.AddProfile(profile);
                var conversationInfoList = CreateConversationsInfo(conversationId, profile);
                conversationsInfoList.Add(conversationInfoList);
                await _chatServiceClient.AddConversation(body);
                _conversationsToCleanup.TryAdd(body.Participants, conversationId);
                _messagesToCleanup.TryAdd(conversationId, body.FirstMessage["Id"]);
                _usersToCleanup.Add(profile);
            }
            var id = _username + "_" + conversationsInfoList[0].Recipient.Username;
            var oldestConversation= await _chatServiceClient.GetConversation(id);
            var conversationsResponse = await _chatServiceClient.GetConversations(_username, 4, oldestConversation.LastModifiedUnixTime);
            
            Assert.Equal(3, conversationsResponse.Conversations.Count);
            Assert.True(IsSortedInDecreasedOrderOfUnixTime(conversationsResponse.Conversations));
            Assert.Null(conversationsResponse.NextUri);
            foreach (var conversation in conversationsResponse.Conversations)
            {
                Assert.Contains(conversation, conversationsInfoList);
            }
        }
        
        [Fact]
        public async Task GetConversationsPaging()
        {
            var conversationsInfoList = new List<ConversationsInfo>();
            var tasks = new List<Task>();
            for (var i = 0; i < 5; ++i)
            {
                var body = CreateRandomAddRequestBody(_username2);
                var id =  body.Participants[0] + "_" + body.Participants[1];
                var profile = CreateAndAddUserProfile(body.Participants[1]);
                await _chatServiceClient.AddProfile(profile);
                var conversationInfoList = CreateConversationsInfo(id, profile);
                conversationsInfoList.Add(conversationInfoList);
                tasks.Add(_chatServiceClient.AddConversation(body));
                _conversationsToCleanup.TryAdd(body.Participants, id);
                _messagesToCleanup.TryAdd(id, body.FirstMessage["Id"]);
                _usersToCleanup.Add(profile);
            }
            await Task.WhenAll(tasks);
            
            var conversationsResponse = await _chatServiceClient.GetConversations(_username2, 3, long.MinValue);
            Assert.Equal(3, conversationsResponse.Conversations.Count);
            Assert.NotNull(conversationsResponse.NextUri);
            foreach (var conversation in conversationsResponse.Conversations)
            {
                Assert.Contains(conversation, conversationsInfoList);
            }
            conversationsInfoList.RemoveAll(s => conversationsResponse.Conversations.Contains(s));
            
            conversationsResponse = await _chatServiceClient.GetConversationsByUri(conversationsResponse.NextUri);
            Assert.Equal(2, conversationsResponse.Conversations.Count);
            Assert.Null(conversationsResponse.NextUri);
            foreach (var conversation in conversationsResponse.Conversations)
            {
                Assert.Contains(conversation, conversationsInfoList);
            }
        }
        
        [Fact]
        public async Task AddConversationTwiceReturns201()
        {
            var addConversationRequestBody = CreateRandomAddRequestBody(Guid.NewGuid().ToString());
            await _chatServiceClient.AddConversation(addConversationRequestBody);
            await _chatServiceClient.AddConversation(addConversationRequestBody);

            var id =  addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1];

            Conversation fetchedConversationFromSender = await _chatServiceClient.GetConversation(id);
            Conversation fetchedConversationFromReceiver = await _chatServiceClient.GetConversation(id);
            Message fetchedMessage = await _chatServiceClient.GetMessage(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Conversation conversation = new Conversation
            {
                Id = "m_" + id,
                Participants = new List<string> {  addConversationRequestBody.Participants[0], addConversationRequestBody.Participants[1]},
                LastModifiedUnixTime = fetchedConversationFromReceiver.LastModifiedUnixTime
            };
            _conversationsToCleanup.TryAdd(conversation.Participants, conversation.Id.Remove(0,2));
            
            Message message = new Message
            {
                Id = addConversationRequestBody.FirstMessage["Id"],
                Text = "ae",
                SenderUsername = addConversationRequestBody.Participants[0],
                UnixTime = fetchedMessage.UnixTime
            };
            _messagesToCleanup.TryAdd(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Assert.Equal(message, fetchedMessage);
            Assert.Equal(conversation,fetchedConversationFromReceiver);
            Assert.Equal(conversation,fetchedConversationFromSender);
        }
        
        [Fact]
        public async Task PostConversationFailedForOneUser()
        {
            var addConversationRequestBody = CreateRandomAddRequestBody(Guid.NewGuid().ToString());
            await _chatServiceClient.AddConversation(addConversationRequestBody);
            var id =  addConversationRequestBody.Participants[0] + "_" + addConversationRequestBody.Participants[1];
            await _chatServiceClient.DeleteConversation(addConversationRequestBody.Participants[1], id);
            await _chatServiceClient.AddConversation(addConversationRequestBody);

            Conversation fetchedConversationFromSender = await _chatServiceClient.GetConversation(id);
            Conversation fetchedConversationFromReceiver = await _chatServiceClient.GetConversation(id);
            Message fetchedMessage = await _chatServiceClient.GetMessage(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Conversation conversation = new Conversation
            {
                Id = "m_" + id,
                Participants = new List<string> {  addConversationRequestBody.Participants[0], addConversationRequestBody.Participants[1]},
                LastModifiedUnixTime = fetchedConversationFromReceiver.LastModifiedUnixTime
            };
            _conversationsToCleanup.TryAdd(conversation.Participants, conversation.Id.Remove(0,2));
            
            Message message = new Message
            {
                Id = addConversationRequestBody.FirstMessage["Id"],
                Text = "ae",
                SenderUsername = addConversationRequestBody.Participants[0],
                UnixTime = fetchedMessage.UnixTime
            };
            _messagesToCleanup.TryAdd(id, addConversationRequestBody.FirstMessage["Id"]);
            
            Assert.Equal(message, fetchedMessage);
            Assert.Equal(conversation,fetchedConversationFromReceiver);
            Assert.Equal(conversation,fetchedConversationFromSender);
        }
        
        private static AddConversationRequestBody CreateRandomAddRequestBody(string username)
        {
            var rand = Guid.NewGuid().ToString();
            var conversation = new AddConversationRequestBody
            {
                Participants = new List<string>{ username, rand},
                FirstMessage = new Dictionary<string, string> { {"Id",rand}, {"Text","ae"}, {"SenderUsername", username} }
            };
            return conversation;
        }

        private static UserProfile CreateAndAddUserProfile(string username)
        {
            var profile = new UserProfile
            {
                Username = username,
                FirstName = "G",
                LastName = "H",
                ProfilePictureId = "profilepic"
            };
            return profile;
        }
        private static ConversationsInfo CreateConversationsInfo(string id, UserProfile profile)
        {
            var conversationInfoList = new ConversationsInfo
            {
                Id = id,
                LastModifiedUnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Recipient = profile
            };
            return conversationInfoList;
        }
        
        private static bool IsSortedInDecreasedOrderOfUnixTime(List<ConversationsInfo> conversations)
        {
            for (int i = 0; i < conversations.Count - 1; i++)
            {
                if (conversations[i].LastModifiedUnixTime < conversations[i+1].LastModifiedUnixTime)
                {
                    return false;
                }
            }
            return true;
        }
    }
}