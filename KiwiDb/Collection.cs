using System;
using KiwiDb.JsonDb;
using KiwiDb.JsonDb.Index;
using KiwiDb.Storage;

namespace KiwiDb
{
    public class Collection : CollectionBase
    {
        public IDatabaseFileProvider FileProvider { get; private set; }

        public Collection(IDatabaseFileProvider fileProvider)
        {
            FileProvider = fileProvider;
            IndexValueFactory = new IndexValueFactory();
        }

        public Collection(string databaseFilePath)
            : this(new DatabaseFileProvider() { Path = databaseFilePath, Timeout = TimeSpan.FromSeconds(30)})
        {
        }

        public IndexValueFactory IndexValueFactory { get; private set; }


        public override T ExecuteReadSession<T>(Func<ISession, T> action)
        {
            using (var session = CreateReadSession())
            {
                return action(session);
            }
        }

        public override T ExecuteWriteSession<T>(Func<ISession, T> action)
        {
            using (var session = CreateUpdateSession())
            {
                var result = action(session);
                session.AutoCommit = true;
                return result;
            }
        }

        public override T ExecuteRead<T>(Func<ICollection, T> action)
        {
            using (var session = CreateReadSession())
            {
                return action(new SessionCollection(session));
            }
        }

        public override T ExecuteWrite<T>(Func<ICollection, T> action)
        {
            using (var session = CreateUpdateSession())
            {
                var result = action(new SessionCollection(session));
                session.AutoCommit = true;
                return result;
            }
        }

        protected ISession CreateReadSession()
        {
            return CreateSession(false);
        }

        protected ISession CreateUpdateSession()
        {
            return CreateSession(true);
        }

        private ISession CreateSession(bool writable)
        {
            var blocks = writable
                             ? FileStreamBlockCollection.CreateWrite(FileProvider)
                             : FileStreamBlockCollection.CreateRead(FileProvider);
            try
            {
                return new Session(blocks, IndexValueFactory);
            }
            catch (Exception)
            {
                blocks.Dispose();
                throw;
            }
        }
    }
}