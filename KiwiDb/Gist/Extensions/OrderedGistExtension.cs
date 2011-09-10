using System.Collections.Generic;
using System.IO;
using KiwiDb.Gist.Tree;

namespace KiwiDb.Gist.Extensions
{
    public class OrderedGistExtension<TKey, TValue> : IGistExtension<TKey, TValue>
    {
        public OrderedGistExtension(IOrderedGistType<TKey> keyType, IOrderedGistType<TValue> valueType)
        {
            KeyType = keyType;
            ValueType = valueType;
        }

        public IOrderedGistType<TKey> KeyType { get; private set; }
        public IOrderedGistType<TValue> ValueType { get; private set; }

        #region IGistExtension<TKey,TValue> Members

        public IGistLeafRecords<TKey, TValue> CreateLeafRecords(IEnumerable<KeyValuePair<TKey, TValue>> records)
        {
            return new OrderedGistLeafRecords<TKey, TValue>(records, KeyType, ValueType);
        }

        public IGistLeafRecords<TKey, TValue> CreateLeafRecords(BinaryReader reader)
        {
            return new OrderedGistLeafRecords<TKey, TValue>(reader, KeyType, ValueType);
        }

        public IGistIndexRecords<TKey> CreateIndexRecords(IEnumerable<KeyValuePair<TKey, int>> records)
        {
            return new OrderedGistIndexRecords<TKey>(records, KeyType);
        }

        public IGistIndexRecords<TKey> CreateIndexRecords(BinaryReader reader)
        {
            return new OrderedGistIndexRecords<TKey>(reader, KeyType);
        }

        #endregion
    }
}