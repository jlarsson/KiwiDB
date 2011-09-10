using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Util;

namespace KiwiDb.Gist.Extensions
{
    public class OrderedGistIndexRecords<TKey> : OrderedGistRecords<TKey, int>, IGistIndexRecords<TKey>
    {
        public OrderedGistIndexRecords(BinaryReader reader, IOrderedGistType<TKey> keyType)
            : base(reader, keyType, GistIntType.Default)
        {
        }

        public OrderedGistIndexRecords(IEnumerable<KeyValuePair<TKey, int>> records, IOrderedGistType<TKey> keyType)
            : base(records, keyType, GistIntType.Default)
        {
        }

        #region IGistIndexRecords<TKey> Members

        public IEnumerable<KeyValuePair<TKey, int>> GetRange(TKey key)
        {
            var range = this.EqualRange(kv => kv.Key, key, KeyType.Comparer);

            var first = range.Item1;
            var last = range.Item2 + 1;

            while ((first < last) && (first < Count))
            {
                yield return this[first++];
            }
        }

        public KeyValuePair<TKey, int> RemoveInsertRecord(TKey key)
        {
            var index = Math.Min(Count - 1, this.LowerBound(kv => kv.Key, key, KeyType.Comparer));
            var record = this[index];
            RemoveAt(index);
            return record;
        }

        public override IEnumerable<IGistRecords<TKey, int>> Split()
        {
            if (Count < 2)
            {
                yield return this;
            }
            else
            {
                var mid = Count/2;
                yield return new OrderedGistIndexRecords<TKey>(this.Take(mid).ToList(), KeyType);
                yield return new OrderedGistIndexRecords<TKey>(this.Skip(mid).ToList(), KeyType);
            }
        }

        #endregion
    }
}