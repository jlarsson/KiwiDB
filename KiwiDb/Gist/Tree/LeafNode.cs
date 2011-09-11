using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;
using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public class LeafNode<TKey, TValue> : Node<TKey, TValue>
    {
        public LeafNode(IGistConfig<TKey, TValue> config, IGistLeafRecords<TKey, TValue> records)
            : base(config, null)
        {
            Records = records;
            Block = config.Blocks.AllocateBlock(GetBytes());
        }

        public LeafNode(IGistConfig<TKey, TValue> config, IBlock block, IGistLeafRecords<TKey, TValue> records)
            : base(config, block)
        {
            Records = records;
        }

        public IGistLeafRecords<TKey, TValue> Records { get; private set; }

        public override TKey MaxKey
        {
            get { return Records.GetMaxKey(); }
        }

        protected override IEnumerable<INode<TKey, TValue>> Split()
        {
            return Records.Split().Select(CreateNode);
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Scan()
        {
            return Records;
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            return Records.Find(key);
        }

        public override bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            PrepareUpdate(key, value, new UpdateActions<TKey, TValue>(Records, key));

            Records.Insert(new KeyValuePair<TKey, TValue>(key, value));
            return HandleUpdate(replaced);
        }

        public override bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            if (Records.Remove(key, filter))
            {
                return HandleUpdate(replaced);
            }
            return false;
        }

        public override void Drop()
        {
            FreeBlock(Block.BlockId);
        }

        protected override BlockHeader CreateBlockHeader()
        {
            return new BlockHeader {Flags = NodeFlags.IsLeafNode};
        }

        protected override void WriteRecords(BinaryWriter writer)
        {
            Records.Write(writer);
        }

        public static int CreateRoot(IGistConfig<TKey, TValue> config, KeyValuePair<TKey, TValue> record)
        {
            return new LeafNode<TKey, TValue>(
                config,
                config.Ext.CreateLeafRecords(new[] {record}))
                .Block.BlockId;
        }
    }
}