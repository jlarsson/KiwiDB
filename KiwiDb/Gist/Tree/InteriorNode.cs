using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;

namespace KiwiDb.Gist.Tree
{
    public class InteriorNode<TKey, TValue> : Node<TKey, TValue>
    {
        public InteriorNode(IGistConfig<TKey, TValue> config, byte[] data) : base(config, data)
        {
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Scan()
        {
            return ReadRecords().Select(kv => GetNode(kv.Value)).SelectMany(n => n.Scan());
        }

        public override IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            return from child in ReadRecords().GetRange(key)
                   from r in GetNode(child.Value).Find(key)
                   select r;
        }

        public override bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var records = ReadRecords();
            var addedRecords = new List<KeyValuePair<TKey, int>>();

            var insertRecord = records.RemoveInsertRecord(key);
            var insertNode = GetNode(insertRecord.Value);
            insertNode.Insert(key, value, addedRecords.AddRange);

            if (addedRecords.Count > 0)
            {
                FreeBlock(insertRecord.Value);
                records.Insert(addedRecords);
                return HandleUpdate(records, replaced);
            }
            return false;
        }

        public override bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var records = ReadRecords();
            var addedRecords = new List<KeyValuePair<TKey, int>>();
            records.Remove(key,
                           kv => GetNode(kv.Value).Remove(key, filter,
                                                          added =>
                                                              {
                                                                  FreeBlock(kv.Value);
                                                                  addedRecords.AddRange(added);
                                                              }
                                     ));

            if (addedRecords.Count > 0)
            {
                records.Insert(addedRecords);
                return HandleUpdate(records, replaced);
            }
            return false;
        }

        protected override BlockHeader CreateBlockHeader()
        {
            return new BlockHeader {Flags = NodeFlags.IsInteriorNode};
        }

        protected IGistIndexRecords<TKey> ReadRecords()
        {
            using (var stream = new MemoryStream(Data, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    ReadBlockHeader(reader);
                    return CreateIndexRecords(reader);
                }
            }
        }

        public static int CreateRoot(IGistConfig<TKey, TValue> config, IList<KeyValuePair<TKey, int>> records)
        {
            return
                config.Blocks.AllocateBlock(
                    new InteriorNode<TKey, TValue>(config, null).SaveRecords(config.Ext.CreateIndexRecords(records))).
                    BlockId;
        }
    }
}