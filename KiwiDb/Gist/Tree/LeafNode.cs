using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;

namespace KiwiDb.Gist.Tree
{
    public class LeafNode<TKey, TValue> : Node<TKey, TValue>
    {
        public LeafNode(IGistConfig<TKey, TValue> config, byte[] data)
            : base(config, data)
        {
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Scan()
        {
            return ReadRecords();
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            return ReadRecords().Find(key);
        }

        public override bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var records = ReadRecords();

            PrepareUpdate(key, value, new UpdateActions(records, key));

            records.Insert(new KeyValuePair<TKey, TValue>(key, value));
            return HandleUpdate(records, replaced);
        }

        public override bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var records = ReadRecords();

            if (records.Remove(key, filter))
            {
                return HandleUpdate(records, replaced);
            }
            return false;
        }

        protected override BlockHeader CreateBlockHeader()
        {
            return new BlockHeader {Flags = NodeFlags.IsLeafNode};
        }

        private IGistLeafRecords<TKey, TValue> ReadRecords()
        {
            using (var stream = new MemoryStream(Data, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    ReadBlockHeader(reader);
                    return CreateLeafRecords(reader);
                }
            }
        }

        public static int CreateRoot(IGistConfig<TKey, TValue> config, KeyValuePair<TKey, TValue> record)
        {
            return config.Blocks.AllocateBlock(new LeafNode<TKey, TValue>(config, null).SaveRecords(
                config.Ext.CreateLeafRecords(new[] {record}))).BlockId;
        }

        #region Nested type: UpdateActions

        public class UpdateActions : IUpdateActions
        {
            private readonly TKey _key;
            private readonly IGistLeafRecords<TKey, TValue> _records;

            public UpdateActions(IGistLeafRecords<TKey, TValue> records, TKey key)
            {
                _records = records;
                _key = key;
            }

            #region IUpdateActions Members

            public void FailIfKeyExists()
            {
                if (_records.Find(_key).Take(1).Count() > 0)
                {
                    throw new ArgumentException(string.Format("A record with key \"{0}\" already exists", _key));
                }
            }

            public void UpdateExistingKey()
            {
                _records.Remove(_key, kv => true);
            }

            public void AppendNewKey()
            {
                // Do nothing
            }

            #endregion
        }

        #endregion
    }
}