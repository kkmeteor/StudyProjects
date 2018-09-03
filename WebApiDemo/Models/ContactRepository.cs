using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApiDemo.Models
{
    public class ContactRepository
    {
        MongoServer _server = null;
        MongoDatabase _database = null;
        MongoCollection _contacts = null;

        public ContactRepository(string connection)
        {
            if (string.IsNullOrWhiteSpace(connection))
            {
                connection = "mongodb://localhost:27017";
            }

            _server = new MongoClient(connection).GetServer();
            _database = _server.GetDatabase("Contacts");
            _contacts = _database.GetCollection("contacts");

            // Reset database and add some default entries
            _contacts.RemoveAll();
            for (int index = 1; index < 5; index++)
            {
                Contact contact1 = new Contact
                {
                    Email = string.Format("test{0}@example.com", index),
                    Name = string.Format("test{0}", index),
                    Phone = string.Format("{0}{0}{0} {0}{0}{0} {0}{0}{0}{0}", index)
                };
                AddContact(contact1);
            }
        }

        public Contact AddContact(Contact item)
        {
            item.Id = ObjectId.GenerateNewId().ToString();
            item.LastModified = DateTime.UtcNow;
            _contacts.Insert(item);
            return item;
        }

        public bool RemoveContact(string id)
        {
            IMongoQuery query = Query.EQ("_id", id);
            WriteConcernResult result = _contacts.Remove(query);
            return result.DocumentsAffected == 1;
        }

        public bool UpdateContact(string id, Contact item)
        {
            IMongoQuery query = Query.EQ("_id", id);
            item.LastModified = DateTime.UtcNow;
            IMongoUpdate update = Update
                .Set("Email", item.Email)
                .Set("LastModified", DateTime.UtcNow)
                .Set("Name", item.Name)
                .Set("Phone", item.Phone);
            WriteConcernResult result = _contacts.Update(query, update);
            return result.UpdatedExisting;
        }
        public IEnumerable GetAllContacts()
        {
            return _contacts.FindAllAs<Contact>();
        }

        public Contact GetContact(string id)
        {
            IMongoQuery query = Query.EQ("_id", id);
            return _contacts.FindAs<Contact>(query).FirstOrDefault();
        }
    }
}