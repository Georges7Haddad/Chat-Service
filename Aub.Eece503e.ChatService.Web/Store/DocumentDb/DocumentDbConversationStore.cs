using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aub.Eece503e.ChatService.Web.Store.DocumentDb
{
    public class DocumentDbConversationStore : IConversationStore
    {

        private readonly IDocumentClient _documentClient;
        private readonly IOptions<DocumentDbSettings> _options;

        private Uri DocumentCollectionUri =>
            UriFactory.CreateDocumentCollectionUri(_options.Value.DatabaseName, _options.Value.CollectionName);

        public DocumentDbConversationStore(IDocumentClient documentClient, IOptions<DocumentDbSettings> options)
        {
            _documentClient = documentClient;
            _options = options;
        }

        public async Task<Conversation> GetConversation(string username, string conversationId)
        {
            try
            {
                var entity = await _documentClient.ReadDocumentAsync<DocumentDbConversationEntity>(
                    CreateDocumentUri("m_" + conversationId),
                    new RequestOptions {PartitionKey = new PartitionKey("c_" + username)});
                return ToConversation(entity);
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new ConversationNotFoundException($"Conversation with id {conversationId} was not found");
                throw new StorageErrorException($"Failed to retrieve conversation {conversationId} from User {username}",
                    e, (int)e.StatusCode);
            }
        }

        public async Task AddConversation(Conversation conversation)
        {
            try
            {
                var senderUsername = "c_" + conversation.Participants[0];
                var receiverUsername = "c_" + conversation.Participants[1];
                var senderEntity = ToEntity(senderUsername, conversation);
                var receiverEntity = ToEntity(receiverUsername, conversation);
                try
                {
                    await _documentClient.CreateDocumentAsync(DocumentCollectionUri, senderEntity);
                }
                catch (DocumentClientException e)
                {
                    if (e.StatusCode != HttpStatusCode.Conflict) 
                        throw new StorageErrorException($"Failed to create conversation {conversation.Id}", 
                            e, (int)e.StatusCode);
                }
                await _documentClient.CreateDocumentAsync(DocumentCollectionUri, receiverEntity);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                    throw new ConversationAlreadyExistsException($"Conversation {conversation.Id} already exists");
                throw new StorageErrorException($"Failed to create conversation {conversation.Id}", 
                    e, (int)e.StatusCode);
            }
        }

        public async Task DeleteConversation(string username, string conversationId)
        {
            try
            {
                await _documentClient.DeleteDocumentAsync(CreateDocumentUri("m_" + conversationId),
                new RequestOptions {PartitionKey = new PartitionKey("c_" + username)});
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new ConversationNotFoundException($"Conversation with id {conversationId} was not found");
                throw new StorageErrorException($"Failed to delete conversation {conversationId} from User {username}",
                    e, (int)e.StatusCode);
            }
        }

        public async Task UpdateConversation(long unixTime, string conversationId)
        {
            try
            {
                string[] usernames = conversationId.Split('_');

                var conversation = new Conversation
                {
                    Id = "m_" + conversationId,
                    LastModifiedUnixTime = unixTime,
                    Participants = new List<string>
                    {
                        usernames[0], usernames[1]
                    }
                };
                
                await Task.WhenAll( _documentClient.UpsertDocumentAsync(DocumentCollectionUri,
                        ToEntity("c_" + conversation.Participants[0], conversation),
                        new RequestOptions {PartitionKey = new PartitionKey("c_" + conversation.Participants[0])}),
                    
                    _documentClient.UpsertDocumentAsync(DocumentCollectionUri,
                        ToEntity("c_" + conversation.Participants[1], conversation),
                        new RequestOptions {PartitionKey = new PartitionKey("c_" + conversation.Participants[1])}));
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new ConversationNotFoundException($"Conversation with id {conversationId} was not found");
                throw new StorageErrorException($"Failed to update conversation {conversationId}",
                    e, (int)e.StatusCode);
            }
        }
        public async Task<GetConversationsResult> GetConversations(string username, string continuationToken, int limit, long lastSeenConversationTime)
        {
            try
            {
                var feedOptions = new FeedOptions
                {
                    MaxItemCount = limit,
                    EnableCrossPartitionQuery = false,
                    RequestContinuation = continuationToken,
                    PartitionKey = new PartitionKey("c_" + username)
                };

                IQueryable<DocumentDbConversationEntity> query = _documentClient.CreateDocumentQuery<DocumentDbConversationEntity>(DocumentCollectionUri, feedOptions)
                    .Where(conversation => conversation.LastModifiedUnixTime > lastSeenConversationTime).OrderByDescending(entity => entity.LastModifiedUnixTime);

                FeedResponse<DocumentDbConversationEntity> feedResponse = await query.AsDocumentQuery().ExecuteNextAsync<DocumentDbConversationEntity>();

                var conversationsInfo = new List<ConversationsInfo>();
                var conversationsList = feedResponse.Select(ToConversation).ToList();

                foreach (var conversation in conversationsList)
                {
                    var conversationInfo = new ConversationsInfo
                    {
                        Id = conversation.Id.Remove(0,2),
                        LastModifiedUnixTime = conversation.LastModifiedUnixTime,
                        Recipient = new UserProfile()
                    };
                    conversationsInfo.Add(conversationInfo);
                }
                
                return new GetConversationsResult
                {
                    ContinuationToken = feedResponse.ResponseContinuation,
                    Conversations = conversationsInfo
                };
            }
            catch (DocumentClientException e)
            {
                if(e.StatusCode == HttpStatusCode.NotFound)
                    throw new ConversationNotFoundException($"{username} was not found");
                throw new StorageErrorException($"Failed to retrieve conversations from User {username}",
                    e, (int)e.StatusCode);
            }
        }

        private Uri CreateDocumentUri(string documentId)
        {
            return UriFactory.CreateDocumentUri(_options.Value.DatabaseName, _options.Value.CollectionName, documentId);
        }

        private static DocumentDbConversationEntity ToEntity(string username, Conversation conversation)
        {
            return new DocumentDbConversationEntity
            {
                PartitionKey = username,
                Id = conversation.Id,
                Participants = conversation.Participants,
                LastModifiedUnixTime = conversation.LastModifiedUnixTime
            };
        }

        private static Conversation ToConversation(DocumentDbConversationEntity entity)
        {
            return new Conversation
            {
                Id = entity.Id,
                Participants = entity.Participants,
                LastModifiedUnixTime = entity.LastModifiedUnixTime
            };
        }
    }

    class DocumentDbConversationEntity
    {
        public string PartitionKey { get; set; }

        [JsonProperty("id")] 
        public string Id { get; set; }
        public List<string> Participants { get; set; }
        public long LastModifiedUnixTime { get; set; }
    }
}