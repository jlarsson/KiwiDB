using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Util;

namespace KiwiDb.Gist.Extensions
{
    public class OrderedGistLeafRecords<TKey, TValue> : OrderedGistRecords<TKey, TValue>, IGistLeafRecords<TKey, TValue>
    {
        public OrderedGistLeafRecords(BinaryReader reader, IOrderedGistType<TKey> keyType,
                                      IOrderedGistType<TValue> valueType)
            : base(reader, keyType, valueType)
        {
        }

        public OrderedGistLeafRecords(IEnumerable<KeyValuePair<TKey, TValue>> records, IOrderedGistType<TKey> keyType,
                                      IOrderedGistType<TValue> valueType) : base(records, keyType, valueType)
        {
        }

        #region IGistLeafRecords<TKey,TValue> Members

        public IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            var range = this.EqualRange(kv => kv.Key, key, KeyType.Comparer);
            for (var i = range.Item1; i < range.Item2; ++i)
            {
                yield return this[i];
            }
/*
            var index = this.LowerBound(kv => kv.Key, key, KeyType.Comparer);
            if (index < Count)
            {
                if (KeyType.Comparer.Compare(key, this[index].Key) == 0)
                {
                    yield return this[index];
                }
            }
 */
        }

        public override IEnumerable<IGistRecords<TKey, TValue>> Split()
        {
            if (Count < 2)
            {
                yield return this;
            }
            else
            {
                var mid = Count/2;
                yield return new OrderedGistLeafRecords<TKey, TValue>(this.Take(mid).ToList(), KeyType, ValueType);
                yield return new OrderedGistLeafRecords<TKey, TValue>(this.Skip(mid).ToList(), KeyType, ValueType);
            }
        }

        #endregion
    }
}