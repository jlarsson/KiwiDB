using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;

namespace KiwiDb.Gist.Tree
{
    public abstract class Node<TKey, TValue> : INode<TKey, TValue>
    {
        protected Node(IGistConfig<TKey, TValue> config, byte[] data)
        {
            Config = config;
            Data = data;
        }

        public IGistConfig<TKey, TValue> Config { get; private set; }

        public byte[] Data { get; private set; }

        #region INode<TKey,TValue> Members

        public abstract IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key);

        public abstract IEnumerable<KeyValuePair<TKey, TValue>> Scan();

        public abstract bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced);

        public abstract bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced);

        #endregion

        public static INode<TKey, TValue> GetNode(IGistConfig<TKey, TValue> config, int blockId)
        {
            var block = config.Blocks.GetBlock(blockId);

            var header = GetBlockHeader(block.Data);

            if ((header.Flags & NodeFlags.IsLeafNode) == NodeFlags.IsLeafNode)
            {
                return new LeafNode<TKey, TValue>(config, block.Data);
            }
            if ((header.Flags & NodeFlags.IsInteriorNode) == NodeFlags.IsInteriorNode)
            {
                return new InteriorNode<TKey, TValue>(config, block.Data);
            }

            throw new ApplicationException("Bad block header");
        }

        protected INode<TKey, TValue> GetNode(int blockId)
        {
            return GetNode(Config, blockId);
        }

        protected static BlockHeader GetBlockHeader(byte[] bytes)
        {
            return new BlockHeader
                       {
                           Flags = (NodeFlags) BitConverter.ToInt32(bytes, 0)
                       };
        }

        protected BlockHeader ReadBlockHeader(BinaryReader reader)
        {
            return new BlockHeader
                       {
                           Flags = (NodeFlags) reader.ReadInt32(),
                       };
        }

        protected static void WriteBlockHeader(BinaryWriter writer, BlockHeader header)
        {
            writer.Write((int) header.Flags);
        }

        protected byte[] SaveRecords<V>(IGistRecords<TKey, V> records)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    WriteBlockHeader(writer, CreateBlockHeader());
                    records.Write(writer);
                    return stream.ToArray();
                }
            }
        }

        protected abstract BlockHeader CreateBlockHeader();

        protected bool HandleUpdate<V>(IGistRecords<TKey, V> records, Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var newData = SaveRecords(records);
            if (!IsLargeData(newData))
            {
                replaced(
                    new[]
                        {
                            new KeyValuePair<TKey, int>(
                                records.GetMaxKey(),
                                AllocateBlock(newData)
                                )
                        });
            }
            else
            {
                replaced((from recs in records.Split()
                          let data = SaveRecords(recs)
                          select
                              new KeyValuePair<TKey, int>(recs.GetMaxKey(),
                                                          AllocateBlock(data))).ToArray());
            }
            return true;
        }

        private int AllocateBlock(byte[] data)
        {
            return Config.Blocks.AllocateBlock(data).BlockId;
        }

        protected bool IsLargeData(byte[] data)
        {
            return Config.Blocks.IsLargeData(data);
        }

        protected void FreeBlock(int blockId)
        {
            Config.Blocks.FreeBlock(blockId);
        }

        protected IGistIndexRecords<TKey> CreateIndexRecords(BinaryReader reader)
        {
            return Config.Ext.CreateIndexRecords(reader);
        }

        protected IGistLeafRecords<TKey, TValue> CreateLeafRecords(BinaryReader reader)
        {
            return Config.Ext.CreateLeafRecords(reader);
        }

        protected void PrepareUpdate(TKey key, TValue value, IUpdateActions actions)
        {
            Config.UpdateStrategy.PrepareUpdate(key, value, actions);
        }
    }
}