using System;
using System.Threading.Tasks;
using Aub.Eece503e.ChatService.DataContracts;
using Aub.Eece503e.ChatService.Web.Store.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Aub.Eece503e.ChatService.Web.Store.Azure
{
    public class AzureTableProfileStore : IProfileStore
    {
        private readonly CloudTable _profileTable;

        public AzureTableProfileStore(IOptions<AzureStorageSettings> options)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(); 
            _profileTable = tableClient.GetTableReference(options.Value.ProfilesTableName);
        }

        public static ProfileTableEntity ToEntity(UserProfile userProfile)
        {
            return new ProfileTableEntity
            {
                PartitionKey = userProfile.Username,
                RowKey = userProfile.Username,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                ProfilePictureId = userProfile.ProfilePictureId
            };
        }

        private static UserProfile ToProfile(ProfileTableEntity profileTableEntity)
        {
            return new UserProfile
            {
                Username = profileTableEntity.RowKey,
                FirstName = profileTableEntity.FirstName,
                LastName = profileTableEntity.LastName,
                ProfilePictureId = profileTableEntity.ProfilePictureId
            };
        }

        private async Task<ProfileTableEntity> RetrieveProfileEntity(string username)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ProfileTableEntity>(username, username);
            TableResult tableResult = await _profileTable.ExecuteAsync(retrieveOperation);
            var entity = (ProfileTableEntity) tableResult.Result;
            
            if (entity == null)
            {
                throw new ProfileNotFoundException($"UserProfile with username {username} not found");
            }
            return entity;
        }

        public async Task<UserProfile> GetProfile(string username)
        {
            try
            {
                var entity = await RetrieveProfileEntity(username);
                return ToProfile(entity);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not read from azure table",e);
            }
        }

        public async Task AddProfile(UserProfile userProfile)
        {
            var entity = ToEntity(userProfile);
            var insertionOperation = TableOperation.Insert(entity);
            
            try
            {
                await _profileTable.ExecuteAsync(insertionOperation);
            }
            catch (StorageException e)
            {
                if(e.RequestInformation.HttpStatusCode == 409)
                    throw new ProfileAlreadyExistsException($"UserProfile with username {userProfile.Username} already exists");
                throw new StorageErrorException("Could not write to azure table", e);
            }
        }

        public async Task UpdateProfile(UserProfile profile)
        {
            var retrieveProfileEntity = await RetrieveProfileEntity(profile.Username);
            retrieveProfileEntity.FirstName = profile.FirstName;
            retrieveProfileEntity.LastName = profile.LastName;
            retrieveProfileEntity.ProfilePictureId = profile.ProfilePictureId;
            TableOperation updateOperation = TableOperation.Replace(retrieveProfileEntity);

            try
            {
                await _profileTable.ExecuteAsync(updateOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 412)
                {
                    throw new StorageConflictException("Optimistic concurrency failed", e);
                }
                throw new StorageErrorException($"Could not update userProfile with username = {profile.Username}", e);
            }
        }

        public async Task DeleteProfile(string username)
        {
            try
            {
                ProfileTableEntity entity = await RetrieveProfileEntity(username);
                var deleteOperation = TableOperation.Delete(entity);
                await _profileTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not delete from azure table",e);
            }
        }
    }
}