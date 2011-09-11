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

        public virtual IJsonValue Get(string key)
        {
            return ExecuteReadSession(session => session.MasterTable.Find(key).Select(kv => kv.Value).FirstOrDefault());
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

        public virtual IList<KeyValuePair<string, IJsonValue>> Find(IJsonValue filter)
        {
            return ExecuteReadSession(session => session.IndexCatalog.FindIndexedObjects(filter).ToList());
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

        public virtual void EnsureIndex(string memberPath, IndexOptions options)
        {
            ExecuteWriteSession(session =>
                                    {
                                        session.IndexCatalog.EnsureIndex(new IndexDefinition
                                                                             {
                                                                                 Path = memberPath,
                                                                                 Options = options
                                                                             });
                                        return true;
                                    });
        }

        public bool DropIndex(string memberPath)
        {
            return ExecuteWriteSession(session =>
                                    {
                                        return session.IndexCatalog.DropIndex(memberPath);
                                    });
        }

        #endregion

        public abstract T ExecuteReadSession<T>(Func<ISession, T> action);
        public abstract T ExecuteWriteSession<T>(Func<ISession, T> action);
    }
}