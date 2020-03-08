
# Chat Service
This is an ongoing project where I implement a chat web service using ASP.NET CORE. It's a copy of the actual code, which is on Azure Repos, where I use CI/CD through Azure pipelines. All APIs are RESTful and Async.

## Data Contracts
In the data contracts, I have a User Profile with a Username, First Name, Last Name, and Profile Picture Id to link the profile picture to User.
I have an Update Profile Request Body, which use to update a user's profile.
I have a Download Image Response, which has an array of bytes, which is our profile picture when I download it.
I have an Upload Image Response, which has a string:  the profile picture Id.
I'm storing profiles in Azure Tables and Images in Azure Blob storage.
## Controllers
I have 2 controllers:
**Profile Controller**, which implements the Get User Profile, Post User Profile, Put User Profile and Delete User Profile APIs. I also have a function to validate the user's input when adding a profile or updating it.

**Image Controller** which implements the Get Profile Picture, Post Profile Picture, and Delete Profile Picture APIs.

## Stores
I have 2 stores:
**Profile Store** which implements the get, add, update and delete functions that communicate with the azure table.
**Image Store**, which implements the download and upload functions that communicate with the blob storage.
## Testing
An essential aspect of this project is Testing. I've created a client that calls all APIs and implemented integration and unit tests to cover all the project.
Integration tests are used to check that each API is working correctly and catching exceptions properly, like adding a user with an already used username.
In the unit tests, I mocked a nonfunctional database to test some more exceptions.
## Code Coverage
This is a picture of the code coverage of tests.
