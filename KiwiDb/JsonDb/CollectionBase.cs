using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kiwi.Json.Untyped;
using KiwiDb.JsonDb.Index;

namespace KiwiDb.JsonDb
{
    public abstract class CollectionBase : ICollection, ICollectionIndices
    {
        #region ICollection Members

        public ICollectionIndices Indices
        {
            get { return this; }
        }

        public abstract T ExecuteRead<T>(Func<ICollection, T> action);
        public abstract T ExecuteWrite<T>(Func<ICollection, T> action);

        public virtual IList<KeyValuePair<string, IJsonValue>> Find(IJsonValue filter)
        {
            return ExecuteReadSession(session => session.IndexCatalog.FindIndexedObjects(filter).ToList());
        }

        public virtual IJsonValue Get(string key)
        {
            return ExecuteReadSession(session => session.MasterTable.Find(key).Select(kv => kv.Value).FirstOrDefault());
        }

        public virtual IJsonValue Update(string key, IJsonValue value)
        {
            return ExecuteWriteSession(session =>
            {
                var originalObject =
                    session.MasterTable.Find(key).Select(kv => kv.Value).FirstOrDefault();
                session.MasterTable.Insert(key, value);

                session.IndexCatalog.UpdateIndex(key, originalObject, value);
                return originalObject;
            });
        }

        public IJsonValue Remove(string key)
        {
            return ExecuteWriteSession(session =>
                                           {
                                               var deleted = default(IJsonValue);
                                               session.MasterTable.Remove(key, kv =>
                                                                                   {
                                                                                       Debug.Assert(deleted == null);
                                                                                       deleted = kv.Value;
                                                                                       return true;
                                                                                   });
                                               return deleted;
                                           });
        }

        #endregion

        #region ICollectionIndices Members

        public IEnumerable<KeyValuePair<string, IndexOptions>> All
        {
            get { return ExecuteReadSession(session => session.IndexCatalog.EnumerateIndices.ToList()); }
        }

        public void VisitIndex(string memberPath, Action<KeyValuePair<IndexValue, string>> visitor)
        {
            ExecuteReadSession(session =>
                                   {
                                       var index = session.IndexCatalog.GetIndex(memberPath);
                                       if (index != null)
                                       {
                                           index.Visit(visitor);
                                           return true;
                                       }
                                       return false;
                                   });
        }

        public virtual bool EnsureIndex(string memberPath, IndexOptions options)
        {
            return ExecuteWriteSession(session => session.IndexCatalog.EnsureIndex(new IndexDefinition
                                                                                       {
                                                                                           Path = memberPath,
                                                                                           Options = options
                                                                                       }));
        }

        public bool DropIndex(string memberPath)
        {
            return ExecuteWriteSession(session => session.IndexCatalog.DropIndex(memberPath));
        }

        #endregion

        public abstract T ExecuteReadSession<T>(Func<ISession, T> action);
        public abstract T ExecuteWriteSession<T>(Func<ISession, T> action);
    }
}