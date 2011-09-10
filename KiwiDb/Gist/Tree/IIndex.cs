using System;
using System.Collections.Generic;

namespace KiwiDb.Gist.Tree
{
    public interface IIndex<TKey, TValue>
    {
        IEnumerable<KeyValuePair<TKey, TValue>> Scan();
        void Insert(TKey key, TValue value);
        void Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter);
        IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key);
    }
}