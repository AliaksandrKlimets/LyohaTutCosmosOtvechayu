using _222.EF;
using _222.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System.Net;
using Employee = _222.EF.Employee;

namespace _222.Services
{
    public class EmployeeService : IEmployeeService
    {
        IHostingEnvironment _environment;

        private DocumentClient client;
        private DocEmployeeConfig config;
        private Uri collectionUri;

        public EmployeeService(IOptions<DocEmployeeConfig> ConfigAccessor, IHostingEnvironment env)
        {
            _environment = env;
            config = ConfigAccessor.Value;
            client = new DocumentClient(new Uri(config.Endpoint), config.AuthKey);
            collectionUri = UriFactory.CreateDocumentCollectionUri(config.Database, config.EmployeeCollection);
            this.CreateDatabaseIfNotExists(config.Database).Wait();
            this.CreateDocumentCollectionIfNotExists(config.Database, config.EmployeeCollection).Wait();
        }

        public IEnumerable<Employee> GetAll(string name, string sort)

        {
            IEnumerable<Employee> employees= client.CreateDocumentQuery<Employee>(collectionUri).ToList<Employee>();

            if (!string.IsNullOrEmpty(name))
            {
                employees = employees.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "ASC":
                        employees = employees.OrderBy(p => p.Name);
                        break;
                    case "DESC":
                        employees = employees.OrderByDescending(p => p.Name);
                        break;
                    default:
                        employees = employees.OrderBy(p => p.Name);
                        break;
                }
            }
            return employees;
        }

        public void Create(CreateModel model)
        {
            Employee employee = new Employee
            {
                Name = model.Name,
                Surname = model.Surname,
                About = model.About,
                BirthDate = model.BirthDate
            };
            if (model.Image != null)
            {

                string str = GetHashString(model.Image.FileName);
                string path = @"\images\" + str + ".jpg";
                string absolutePath = _environment.WebRootPath + path;
                using (var fileStream = new FileStream(absolutePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
                employee.ImageLocation = path;
            }
            client.CreateDocumentAsync(collectionUri, employee).Wait();
        }

        public Employee Get(int Id)
        {
            try
            {
                Employee item = client.ReadDocumentAsync<Employee>(UriFactory.CreateDocumentUri(config.Database, config.EmployeeCollection, Id.ToString()).ToString()).Result;
                return item;
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is DocumentClientException &&
                    (ae.InnerException as DocumentClientException).StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }

        }

        //public void Edit(EditModel model)
        //{

        //    Employee employee = _context.Employees.Find(model.Id);
        //    employee.Name = model.Name;
        //    employee.Surname = model.Surname;
        //    employee.About = model.About;


        //    if (model.Image != null)
        //    {

        //        string str = GetHashString(model.Image.FileName);
        //        string path = @"\images\" + str + ".jpg";
        //        string absolutePath = _environment.WebRootPath + path;
        //        using (var fileStream = new FileStream(absolutePath, FileMode.Create))
        //        {
        //            model.Image.CopyTo(fileStream);
        //        }
        //        employee.ImageLocation = path;
        //    }
        //    _context.Entry(employee).State = EntityState.Modified;
        //    _context.SaveChanges();
        //}

        public void Delete(int Id)
        {
            client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(config.Database, config.EmployeeCollection, Id.ToString())).Wait();
        }
        private string GetHashString(string s)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(s);
            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = string.Empty;
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);
            return hash;
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
