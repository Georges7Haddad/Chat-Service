using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.Client;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Aub.Eece503e.ChatService.Web.Tests
{
    public class ConversationsControllerTests
    {
        private static Mock<IMessageStore> messageStoreMock = new Mock<IMessageStore>();
        private static Mock<IConversationStore> conversationStoreMock = new Mock<IConversationStore>();

        static TestServer testServer = new TestServer(
            Program.CreateWebHostBuilder(new string[] { })
                .ConfigureTestServices(services =>
                {
                    services.AddSingleton(messageStoreMock.Object);
                    services.AddSingleton(conversationStoreMock.Object);
                }).UseEnvironment("Development"));

        static HttpClient httpClient = testServer.CreateClient();
        static ChatServiceClient chatServiceClient = new ChatServiceClient(httpClient);
        
        private static readonly string _conversationId = "Bleast_Kran";
        private static readonly AddMessageRequestBody _addMessageRequestBody = new AddMessageRequestBody
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Naniard",
            SenderUsername = "Bleast",
        };
        private readonly Message _message = new Message
        {
            Id = _addMessageRequestBody.Id,
            Text = _addMessageRequestBody.Text,
            SenderUsername = _addMessageRequestBody.SenderUsername,
            UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };
        private static readonly AddConversationRequestBody _addConversationRequestBody = new AddConversationRequestBody
        {
            Participants = new List<string>{"Bleast", "Kran"},
            FirstMessage = new Dictionary<string, string> { {"Id", _addMessageRequestBody.Id}, {"Text","Naniard"}, {"SenderUsername", "Bleast"} }
        };
        private readonly Conversation _conversation = new Conversation
        {
            Id = "m_" + _conversationId,
            Participants = _addConversationRequestBody.Participants,
            LastModifiedUnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        public async Task ConversationsControllerAddMessageStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            messageStoreMock.Setup(store => store.AddMessage(_conversationId, _message))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));

            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.AddMessage(_conversationId, _addMessageRequestBody));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        public async Task ConversationsControllerGetMessagesStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            messageStoreMock.Setup(store => store.GetMessages(_conversationId, null,3, long.MinValue))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));
            
            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.GetMessages(_conversationId, 3, Int64.MinValue));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        public async Task ConversationsControllerAddConversationStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            conversationStoreMock.Setup(store => store.AddConversation(_conversation))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));

            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.AddConversation(_addConversationRequestBody));
            Assert.Equal(statusCode, e.StatusCode);
        }
        
        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        public async Task ConversationsControllerGetConversationsStatusCodeOnStorageErrors(HttpStatusCode statusCode)
        {
            conversationStoreMock.Setup(store => store.GetConversations("Bleast", null, 3, long.MinValue))
                .ThrowsAsync(new StorageErrorException("Test Exception", (int)statusCode));

            ChatServiceException e = await Assert.ThrowsAsync<ChatServiceException>(() => chatServiceClient.GetConversations("Bleast", 3, long.MinValue));
            Assert.Equal(statusCode, e.StatusCode);
        }
    }
}