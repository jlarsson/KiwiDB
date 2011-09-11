using System;
using System.Collections.Generic;
using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public interface INode<TKey, TValue>
    {
        TKey MaxKey { get; }
        IBlock Block { get; }


        IEnumerable<KeyValuePair<TKey, TValue>> Scan();
        IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key);
        bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced);
        bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                    Action<IList<KeyValuePair<TKey, int>>> replaced);

        void Drop();
    }
}