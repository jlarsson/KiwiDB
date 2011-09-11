using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Gist.Extensions;
using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public abstract class Node<TKey, TValue> : INode<TKey, TValue>
    {
        protected Node(IGistConfig<TKey, TValue> config, IBlock block)
        {
            Config = config;
            Block = block;
        }

        public IGistConfig<TKey, TValue> Config { get; private set; }
        public IBlock Block { get; set; }

        #region INode<TKey,TValue> Members

        public abstract IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key);

        public abstract TKey MaxKey { get; }

        public abstract IEnumerable<KeyValuePair<TKey, TValue>> Scan();

        public abstract bool Insert(TKey key, TValue value, Action<IList<KeyValuePair<TKey, int>>> replaced);

        public abstract bool Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter,
                                    Action<IList<KeyValuePair<TKey, int>>> replaced);

        public abstract void Drop();

        #endregion

        public static INode<TKey, TValue> GetNode(IGistConfig<TKey, TValue> config, int blockId)
        {
            var block = config.Blocks.GetBlock(blockId);

            if (block.UserData != null)
            {
                return (INode<TKey, TValue>) block.UserData;
            }

            INode<TKey, TValue> node = null;
            using (var stream = new MemoryStream(block.Data, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var header = ReadBlockHeader(reader);
                    if ((header.Flags & NodeFlags.IsLeafNode) == NodeFlags.IsLeafNode)
                    {
                        node = new LeafNode<TKey, TValue>(
                            config,
                            block,
                            config.Ext.CreateLeafRecords(reader)
                            );
                    }
                    else if ((header.Flags & NodeFlags.IsInteriorNode) == NodeFlags.IsInteriorNode)
                    {
                        node = new InteriorNode<TKey, TValue>(
                            config,
                            block,
                            config.Ext.CreateIndexRecords(reader)
                            );
                    }
                }
            }
            if (node == null)
            {
                throw new ApplicationException("Bad block header");
            }
            block.UserData = node;
            return node;
        }

        protected INode<TKey, TValue> CreateNode<TV>(IGistRecords<TKey, TV> recs)
        {
            if (recs is IGistIndexRecords<TKey>)
            {
                return new InteriorNode<TKey, TValue>(Config, (IGistIndexRecords<TKey>)recs);
            }
            if (recs is IGistLeafRecords<TKey, TValue>)
            {
                return new LeafNode<TKey, TValue>(Config, (IGistLeafRecords<TKey, TValue>)recs);
            }
            throw new ApplicationException("Bad records");
        }

        protected INode<TKey, TValue> GetNode(int blockId)
        {
            return GetNode(Config, blockId);
        }

        protected static BlockHeader ReadBlockHeader(BinaryReader reader)
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

        protected bool HandleUpdate(Action<IList<KeyValuePair<TKey, int>>> replaced)
        {
            var newData = GetBytes();
            if (!IsLargeData(newData))
            {
                replaced(
                    new[]
                        {
                            new KeyValuePair<TKey, int>(
                                MaxKey,
                                AllocateBlock(newData)
                                )
                        });
            }
            else
            {
                replaced(Split().Select(n => new KeyValuePair<TKey, int>(n.MaxKey, n.Block.BlockId)).ToArray());
            }
            return true;
        }

        protected abstract IEnumerable<INode<TKey, TValue>> Split();

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

        protected byte[] GetBytes()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    WriteBlockHeader(writer, CreateBlockHeader());
                    WriteRecords(writer);
                    return stream.ToArray();
                }
            }
        }

        protected abstract void WriteRecords(BinaryWriter writer);
    }
}