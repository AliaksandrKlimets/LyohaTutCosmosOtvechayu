using _222.EF;
using _222.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using User = _222.EF.User;

namespace _222.Services
{
    public class UserService :IUserService
{
        private DocumentClient client;
        private DocUserConfig config;
        private Uri collectionUri;

        public UserService(IOptions<DocUserConfig> ConfigAccessor)
        {
            config = ConfigAccessor.Value;
            Console.WriteLine(config.Endpoint);
            Console.WriteLine(config.AuthKey);
            client = new DocumentClient(new Uri(config.Endpoint), config.AuthKey);
            collectionUri = UriFactory.CreateDocumentCollectionUri(config.Database, config.UserCollection);
            this.CreateDatabaseIfNotExists(config.Database).Wait();
            Console.WriteLine("DBG AHR11");
            this.CreateDocumentCollectionIfNotExists(config.Database, config.UserCollection).Wait();
            Console.WriteLine("DBG AHR22");
        }

        public string CreateItem(User NewItem)
        {
            string NewID = Guid.NewGuid().ToString();
            NewItem.Id = NewID;
            client.CreateDocumentAsync(collectionUri, NewItem).Wait();
            return NewID;
        }

        public async Task<User> FindOnLoginAsync(string email, string passwordString)
        {
            List<User> users = client.CreateDocumentQuery<User>(collectionUri).ToList<User>();
            Console.Write("DBG AHR 1");
            foreach (var user in users)
            {
                Console.Write(user.Email);
                Console.Write(user.Password);
                Console.Write("DBG AHR 2");
                if (user.Email.Equals(email) && user.Password.Equals(passwordString))
                {
                    return user;
                }
            }

            return null;
        }



        public async Task<bool> IsEmailExistAsync(string email)
        {
            IEnumerable<User> users = client.CreateDocumentQuery<User>(collectionUri);
            foreach (User user in users) {
                if (user.Email.Equals(email)) return true;
            };
            return false;
        }


        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        new DocumentCollection { Id = collectionName },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
