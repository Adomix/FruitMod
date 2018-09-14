using System;
using System.Configuration;
using FruitMod.Attributes;
using Raven.Client.Documents;

namespace FruitMod.Database
{
    [SetService]
    public class DbService
    {
        private readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateStore);

        private IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateStore()
        {
            var store = new DocumentStore
            {
                Urls = new[] {ConfigurationManager.AppSettings["ip"]},
                Database = "BotDB"
            }.Initialize();

            return store;
        }


        public void StoreObject<T>(T entity, object id)
        {
            using (var session = Store.OpenSession())
            {
                session.Store(entity, $"{id}");
                session.SaveChanges();
            }
        }

        public T GetById<T>(object id)
        {
            using (var session = Store.OpenSession())
            {
                return session.Load<T>($"{id}");
            }
        }

        public void DeleteObject(object id)
        {
            using (var session = Store.OpenSession())
            {
                session.Delete(id);
                session.SaveChanges();
            }
        }
    }
}