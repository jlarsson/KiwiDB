using System;
using System.Collections.Generic;
using System.IO;
using KiwiDb.Util;

namespace KiwiDb.Gist.Extensions
{
    public abstract class OrderedGistRecords<TKey, TValue> : List<KeyValuePair<TKey, TValue>>,
                                                             IGistRecords<TKey, TValue>
    {
        protected OrderedGistRecords(BinaryReader reader, IOrderedGistType<TKey> keyType,
                                     IOrderedGistType<TValue> valueType)
        {
            KeyType = keyType;
            ValueType = valueType;

            var count = reader.ReadInt32();
            Capacity = count;
            for (var i = 0; i < count; ++i)
            {
                Add(new KeyValuePair<TKey, TValue>(keyType.Read(reader), valueType.Read(reader)));
            }
        }

        protected OrderedGistRecords(IEnumerable<KeyValuePair<TKey, TValue>> records, IOrderedGistType<TKey> keyType,
                                     IOrderedGistType<TValue> valueType) : base(records)
        {
            KeyType = keyType;
            ValueType = valueType;
        }

        public IOrderedGistType<TKey> KeyType { get; private set; }
        public IOrderedGistType<TValue> ValueType { get; private set; }

        #region IGistRecords<TKey,TValue> Members

        public TKey GetMaxKey()
        {
            return this[Count - 1].Key;
        }

        public void Insert(KeyValuePair<TKey, TValue> record)
        {
            Insert(
                this.LowerBound(kv => kv.Key, record.Key, KeyType.Comparer),
                record);
        }

        public bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter)
        {
            var range = this.EqualRange(kv => kv.Key, key, KeyType.Comparer);
            var first = range.Item1;
            var last = range.Item2;
            var changed = false;
            while (first < last)
            {
                if (filter(this[first]))
                {
                    changed = true;
                    RemoveAt(first);
                    --last;
                }
                else
                {
                    ++first;
                }
            }
            return changed;
        }

        public abstract IEnumerable<IGistRecords<TKey, TValue>> Split();

        public void Write(BinaryWriter writer)
        {
            writer.Write(Count);
            foreach (var kv in this)
            {
                KeyType.Write(writer, kv.Key);
                ValueType.Write(writer, kv.Value);
            }
        }

        #endregion
    }
}