using System;
using System.Collections.Generic;
using System.IO;

namespace KiwiDb.Gist.Extensions
{
    public interface IGistRecords<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }
        TKey GetMaxKey();
        void Insert(KeyValuePair<TKey, TValue> record);
        bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter);
        IEnumerable<IGistRecords<TKey, TValue>> Split();
        void Write(BinaryWriter writer);
    }

    public static class GistRecordsExtensions
    {
        public static void Insert<TKey, TValue>(this IGistRecords<TKey, TValue> r,
                                                IList<KeyValuePair<TKey, TValue>> records)
        {
            foreach (var kv in records)
            {
                r.Insert(kv);
            }
        }
    }
}