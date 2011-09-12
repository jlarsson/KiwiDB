using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;
using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public class InteriorNode<TKey, TValue> : Node<TKey, TValue>
    {
        public InteriorNode(IGistConfig<TKey, TValue> config, IGistIndexRecords<TKey> records)
            : base(config, null)
        {
            Records = records;
            Block = config.Blocks.AllocateBlock(GetBytes());
            Block.UserData = this;
        }

        public InteriorNode(IGistConfig<TKey, TValue> config, IBlock block, IGistIndexRecords<TKey> records)
            : base(config, block)
        {
            Records = records;
        }

        public override TKey MaxKey
        {
            get { return Records.GetMaxKey(); }
        }

        public IGistIndexRecords<TKey> Records { get; private set; }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Scan()
        {
            return Records.Select(kv => GetNode(kv.Value)).SelectMany(n => n.Scan());
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            return from child in Records.GetRange(key)
                   from r in GetNode(child.Value).Find(key)
                   select r;
        }

        public override bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var addedRecords = new List<KeyValuePair<TKey, int>>();

            var insertRecord = Records.RemoveInsertRecord(key);
            var insertNode = GetNode(insertRecord.Value);
            insertNode.Insert(key, value, addedRecords.AddRange);

            if (addedRecords.Count > 0)
            {
                FreeBlock(insertRecord.Value);
                Records.Insert(addedRecords);
                return HandleUpdate(replaced);
            }
            return false;
        }

        public override bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var addedRecords = new List<KeyValuePair<TKey, int>>();
            Records.Remove(key,
                           kv => GetNode(kv.Value).Remove(key, filter,
                                                          added =>
                                                              {
                                                                  FreeBlock(kv.Value);
                                                                  addedRecords.AddRange(added);
                                                              }
                                     ));

            if (addedRecords.Count > 0)
            {
                Records.Insert(addedRecords);
                return HandleUpdate(replaced);
            }
            return false;
        }

        public override void Drop()
        {
            foreach (var record in Records)
            {
                var blockId = record.Value;
                GetNode(blockId).Drop();
            }
            FreeBlock(Block.BlockId);
        }

        protected override BlockHeader CreateBlockHeader()
        {
            return new BlockHeader {Flags = NodeFlags.IsInteriorNode};
        }

        public override bool IsEmpty
        {
            get { return Records.Count == 0; }
        }

        protected override IEnumerable<INode<TKey, TValue>> Split()
        {
            return from recs in Records.Split()
                   select CreateNode(recs);
        }

        protected override void WriteRecords(BinaryWriter writer)
        {
            Records.Write(writer);
        }

        public static int CreateRoot(IGistConfig<TKey, TValue> config, IList<KeyValuePair<TKey, int>> records)
        {
            return new InteriorNode<TKey, TValue>(
                        config,
                        config.Ext.CreateIndexRecords(records)
                        )
                        .Block.BlockId;
        }
    }
}