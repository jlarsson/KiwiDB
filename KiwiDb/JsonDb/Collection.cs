using System;
using KiwiDb.JsonDb.Index;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb
{
    public class Collection : CollectionBase
    {
        public Collection(string dabaseFilePath)
        {
            DabaseFilePath = dabaseFilePath;
            IndexValueFactory = new IndexValueFactory();
        }

        public IndexValueFactory IndexValueFactory { get; private set; }

        public string DabaseFilePath { get; private set; }

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
                             ? FileStreamBlockCollection.CreateWrite(DabaseFilePath)
                             : FileStreamBlockCollection.CreateRead(DabaseFilePath);
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