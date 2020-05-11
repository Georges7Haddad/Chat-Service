
#  Chat Service

I implemented a chat web service using ASP.NET CORE. It's a copy of the actual code, which is on Azure Repos, where I use CI/CD pipeline. All APIs are Async and most of them are RESTful. The project was deployed on an azure web app.

##  Controllers

**Profile Controller** which implements the Get User Profile, Post User Profile, Put User Profile and Delete User Profile APIs. I also have a function to validate the user's input when adding a profile or updating it.

**Image Controller** which implements the Get Profile Picture, Post Profile Picture, and Delete Profile Picture APIs.

**Conversation Controller** which implements all APIs related to conversations and messages like adding new messages or conversations and listing them. 

  ## Services
 I implemented a service layer for each controller to seperate handling http request from store calls.
 This layer takes care of all business logic. 

##  Stores

**Profile Store** which implements the get, add, update and delete functions that communicate with the azure table.

**Image Store**, which implements the download and upload functions that communicate with the blob storage.

**Conversation Store**, which implements the get, add, update and list conversations functions that communicate with CosmosDB(DocumentDB).

**Message Store**, which implements the get, add, update and list conversations functions that communicate with CosmosDB(DocumentDB).

##  Testing

An essential aspect of this project is Testing. I've created a client that calls all APIs and implemented integration and unit tests to cover all the project.

Integration tests are used to check that each API is working correctly and catching exceptions properly, like adding a user with an already used username.

In the unit tests, I mocked a nonfunctional database to test some more exceptions.

## Features

 - I implemented a middleware to catch all exceptions.
 
 - Each message gets a unique Id, so in case a message was added but the
   client app retried because of an error, the message will only be
   added once. This way, the message will not appear twice in the
   conversation.
 - There is a last seen message time and last seen conversation time so
   when fetching messages or conversations we just need to get the ones
   that have a time bigger than the last seen.
 - Listing conversations and messages also supports paging to get older
   messages/conversations.
 - Adding a message updates a conversation's time. In case the client
   app retries to add the same message, the conversation will get the
   first message's time.
 - I also handle the failing scenario where creating a new conversation
   fails to add the conversation to one of the users and succeeds to add
   it to the other.

