using System;

namespace KiwiDb.JsonDb
{
    public class SessionCollection : CollectionBase
    {
        public SessionCollection(ISession session)
        {
            Session = session;
        }

        public ISession Session { get; set; }

        public override T ExecuteReadSession<T>(Func<ISession, T> action)
        {
            return action(Session);
        }

        public override T ExecuteWriteSession<T>(Func<ISession, T> action)
        {
            return action(Session);
        }

        public override T ExecuteRead<T>(Func<ICollection, T> action)
        {
            return action(this);
        }

        public override T ExecuteWrite<T>(Func<ICollection, T> action)
        {
            return action(this);
        }
    }
}