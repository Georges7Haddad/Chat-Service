using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;

namespace Aub.Eece503e.ChatService.Web.Store.DocumentDb
{
    public class DocumentDbMessageStore : IMessageStore
    {
        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;
        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);
        
        public DocumentDbMessageStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }
        
        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbMessageEntity>(
                    CreateDocumentUri(messageId),
                    new RequestOptions{PartitionKey = new PartitionKey("m_" +conversationId)});
                return ToMessage(entity);
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new MessageNotFoundException($"Message with id {messageId} was not found");
                throw new StorageErrorException($"Failed to retrieve message {messageId} from conversation {conversationId}",
                    e, (int)e.StatusCode);
            }
        }
        
        public async Task AddMessage(string conversationId, Message message)
        {
            try
            {
                var entity = ToEntity("m_" + conversationId, message);
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, entity);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                    throw new MessageAlreadyExistsException($"Message  {message.Id} Already Exists");
                throw new StorageErrorException($"Failed to create conversation {conversationId}", 
                    e, (int)e.StatusCode);
                
            }
        }

        public async Task DeleteMessage(string conversationId, string messageId)
        {
            try
            {
                await _documentClient.DeleteDocumentAsync(CreateDocumentUri(messageId),
                new RequestOptions{PartitionKey = new PartitionKey("m_" + conversationId)});
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new MessageNotFoundException($"Message with id {messageId} was not found");
                throw new StorageErrorException($"Failed to delete message {messageId} from conversation {conversationId}", 
                    e, (int)e.StatusCode);
            }
        }

        public async Task<GetMessagesResult> GetMessages(string conversationId, string continuationToken, int limit, long lastSeenMessageTime)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey("m_" + conversationId)
                };
                
                IQueryable<DocumentDbMessageEntity> query = _documentClient.CreateDocumentQuery<DocumentDbMessageEntity>(DocumentCollectionUri, feedOptions)
                    .Where(message => message.UnixTime > lastSeenMessageTime).OrderByDescending(entity => entity.UnixTime);

                FeedResponse<DocumentDbMessageEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbMessageEntity>();
                
                var messages = new List<MessagesInfo>();
                var messagesList = feedResponse.Select(ToMessage).ToList();

                foreach (var message in messagesList)
                {
                    var messageInfo = new MessagesInfo
                    {
                        Text = message.Text,
                        SenderUsername = message.SenderUsername,
                        UnixTime = message.UnixTime
                    };
                    messages.Add(messageInfo);
                }
                
                return new GetMessagesResult
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Messages = messages
                };
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new MessageNotFoundException($"Conversation with id {conversationId} was not found");
                throw new StorageErrorException($"Failed to get {limit} messages from conversation {conversationId}", 
                    e, (int)e.StatusCode);
            }
        }

        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }

        private static DocumentDbMessageEntity ToEntity(string conversationId, Message message)
        {
            return new DocumentDbMessageEntity
            {
                PartitionKey = conversationId,
                Id = message.Id,
                Text = message.Text,
                SenderUsername = message.SenderUsername,
                UnixTime = message.UnixTime
            };
        }

        private static Message ToMessage(DocumentDbMessageEntity entity)
        {
            return new Message
            {
                Id = entity.Id,
                Text = entity.Text,
                SenderUsername = entity.SenderUsername,
                UnixTime = entity.UnixTime
            };
        }
    }
    
    class DocumentDbMessageEntity
    {
        public string PartitionKey { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public long UnixTime { get; set; }
    }
}