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
    public abstract class MessageControllerEndToEndTests<TFixture> : IClassFixture<TFixture>, IAsyncLifetime where TFixture : class, IEndToEndTestsFixture
    {
        private static string _conversationId = Guid.NewGuid() + "_" + Guid.NewGuid();
        private readonly ConcurrentBag<string> _messagesToCleanup = new ConcurrentBag<string>();
        private readonly IChatServiceClient _chatServiceClient;

        public MessageControllerEndToEndTests(TFixture tfixture)
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
            foreach(var messageId in _messagesToCleanup)
            {
                var task = _chatServiceClient.DeleteMessage(_conversationId, messageId);
                tasks.Add(task);
            }
            string[] usernames = _conversationId.Split('_');
            tasks.Add(_chatServiceClient.DeleteConversation(usernames[0], _conversationId));
            tasks.Add(_chatServiceClient.DeleteConversation(usernames[1], _conversationId));
            await Task.WhenAll(tasks);
        }
        
        [Fact]
        public async Task PostGetMessage()
        {
            var addMessageRequestBody = CreateRandomAddMessageBody();
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            var message = AddMessageBodyToMessage(addMessageRequestBody);
            _messagesToCleanup.Add(message.Id);
        
            var fetchedMessage = await _chatServiceClient.GetMessage(_conversationId, addMessageRequestBody.Id);
            Assert.Equal(message, fetchedMessage);
        }
        
        [Fact]
        public async Task UpdateConversationTimeWhenAddingMessage()
        {
            var addMessageRequestBody = CreateRandomAddMessageBody();
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            var message = AddMessageBodyToMessage(addMessageRequestBody);
            _messagesToCleanup.Add(message.Id);
        
            var fetchedMessage = await _chatServiceClient.GetMessage(_conversationId, addMessageRequestBody.Id);
            Assert.Equal(message, fetchedMessage);
            var fetchedConversation = await _chatServiceClient.GetConversation(_conversationId);
            Assert.Equal(fetchedMessage.UnixTime, fetchedConversation.LastModifiedUnixTime);
        }
        
        [Fact]
        public async Task UpdateConversationTimeWhenAddingMessageTwiceAndAlreadyExists()
        {
            var addMessageRequestBody = CreateRandomAddMessageBody();
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            var message = AddMessageBodyToMessage(addMessageRequestBody);
            _messagesToCleanup.Add(message.Id);
        
            var fetchedMessage = await _chatServiceClient.GetMessage(_conversationId, addMessageRequestBody.Id);
            Assert.Equal(message, fetchedMessage);
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            var fetchedConversation = await _chatServiceClient.GetConversation(_conversationId);
            Assert.Equal(fetchedMessage.UnixTime, fetchedConversation.LastModifiedUnixTime);
        }
        
        [Fact]
        public async Task DeleteMessage()
        {
            var addMessageRequestBody = CreateRandomAddMessageBody();
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            await _chatServiceClient.DeleteMessage(_conversationId, addMessageRequestBody.Id);
        
            var exception = await Assert.ThrowsAsync<ChatServiceException>(()
                => _chatServiceClient.GetMessage(_conversationId, addMessageRequestBody.Id));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task GetMessagesPaging()
        {
            var messagesInfoList = new List<MessagesInfo>();
            var tasks = new List<Task>();
            for (var i = 0; i < 4; ++i)
            {
                var addMessageRequestBody = CreateRandomAddMessageBody();
                var message = AddMessageBodyToMessage(addMessageRequestBody);
                var messageInfo = MessageToMessageInfo(message);
                messagesInfoList.Add(messageInfo);
                tasks.Add(_chatServiceClient.AddMessage(_conversationId, addMessageRequestBody));
                _messagesToCleanup.Add(addMessageRequestBody.Id);
            }
            await Task.WhenAll(tasks);
            
            var messagesResponse = await _chatServiceClient.GetMessages(_conversationId, 3, long.MinValue);
            Assert.Equal(3, messagesResponse.Messages.Count);
            Assert.NotEmpty(messagesResponse.NextUri);
            foreach (var message in messagesResponse.Messages)
            {
                Assert.Contains(message, messagesInfoList);
                messagesInfoList.Remove(message);
            }

            messagesResponse = await _chatServiceClient.GetMessagesByUri(messagesResponse.NextUri);
            Assert.Single(messagesResponse.Messages);
            Assert.Null(messagesResponse.NextUri);
            foreach (var message in messagesResponse.Messages)
            {
                Assert.Contains(message, messagesInfoList);
            }
        }
        
        [Fact]
        public async Task GetNewMessages()
        {
            var messagesInfoList = new List<MessagesInfo>();
            var oldestMessageId = "";
            for (var i = 0; i < 4; i++)
            {
                var addMessageRequestBody = CreateRandomAddMessageBody();
                if(i == 0)
                    oldestMessageId = addMessageRequestBody.Id;
                var message = AddMessageBodyToMessage(addMessageRequestBody);
                var messageInfo = MessageToMessageInfo(message);
                messagesInfoList.Add(messageInfo);
                await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
                _messagesToCleanup.Add(addMessageRequestBody.Id);
            }
            var oldestMessage= await _chatServiceClient.GetMessage(_conversationId, oldestMessageId);

            var messagesResponse = await _chatServiceClient.GetMessages(_conversationId, 4, oldestMessage.UnixTime);
            Assert.Equal(3, messagesResponse.Messages.Count);
            Assert.Null(messagesResponse.NextUri);
            Assert.True(IsSortedInDecreasedOrderOfUnixTime(messagesResponse.Messages));
            
            foreach (var message in messagesResponse.Messages)
            {
                Assert.Contains(message, messagesInfoList);
            }
        }
        
        [Fact]
        public async Task PostSameMessageTwiceReturns201()
        {
            var addMessageRequestBody = CreateRandomAddMessageBody();
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            await _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody);
            var fetchedMessage = await _chatServiceClient.GetMessage(_conversationId, addMessageRequestBody.Id);
            var message = AddMessageBodyToMessage(addMessageRequestBody);
            _messagesToCleanup.Add(message.Id);
            Assert.Equal(message, fetchedMessage);
        }
        
        [Fact]
        public async Task PostEmptyMessageException()
        {
            // We create the conversation or the dispose will throw not found
            var rand = Guid.NewGuid().ToString();
            string[] usernames = _conversationId.Split('_');
            var addConversationRequestBody = new AddConversationRequestBody
            {
                Participants = new List<string>{ usernames[0], usernames[1]},
                FirstMessage = new Dictionary<string, string> { {"Id",rand}, {"Text","ae"}, {"SenderUsername", usernames[0]} }
            };
            await _chatServiceClient.AddConversation(addConversationRequestBody);
            _messagesToCleanup.Add(rand);
            
            
            var id = Guid.NewGuid().ToString();
            var addMessageRequestBody = new AddMessageRequestBody
            {
                Id = id,
                SenderUsername = "kamil",
                Text = "",
            };
            
            var exception = await Assert.ThrowsAsync<ChatServiceException>(
                () => _chatServiceClient.AddMessage(_conversationId, addMessageRequestBody));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        }
        
        private static AddMessageRequestBody CreateRandomAddMessageBody()
        {
            var rand = Guid.NewGuid().ToString();
            var addMessageRequestBody = new AddMessageRequestBody
            {
                Id = rand, SenderUsername = "kamil", Text = "Hola"
            };
            return addMessageRequestBody;
        }
        
        private static Message AddMessageBodyToMessage(AddMessageRequestBody addMessageRequestBody)
        {
            var message = new Message
            {
                Id = addMessageRequestBody.Id, SenderUsername = addMessageRequestBody.SenderUsername, Text = addMessageRequestBody.Text, UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
            return message;
        }
        
         private static MessagesInfo MessageToMessageInfo(Message message)
        {
            var messagesInfo = new MessagesInfo
            {
                SenderUsername = message.SenderUsername, Text = message.Text, UnixTime = message.UnixTime
            };
            return messagesInfo;
        }
        
        private static bool IsSortedInDecreasedOrderOfUnixTime(List<MessagesInfo> messages)
        {
            for (int i = 0; i < messages.Count - 1; i++)
            {
                if (messages[i].UnixTime < messages[i+1].UnixTime)
                {
                    return false;
                }
            }
            return true;
        }
    }
}