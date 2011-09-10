using System;
using System.Collections.Generic;
using System.Linq;
using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public class Gist<TKey, TValue> : IIndex<TKey, TValue>
    {
        public Gist(IBlockReference blockReference, IGistConfig<TKey, TValue> config)
        {
            BlockReference = blockReference;
            Config = config;
        }

        public IBlockReference BlockReference { get; private set; }
        public IGistConfig<TKey, TValue> Config { get; private set; }

        #region IIndex<TKey,TValue> Members

        public IEnumerable<KeyValuePair<TKey, TValue>> Scan()
        {
            if (BlockReference.BlockId != 0)
            {
                var root = Node<TKey, TValue>.GetNode(Config, BlockReference.BlockId);
                foreach (var record in root.Scan())
                {
                    yield return record;
                }
            }
        }

        public void Insert(TKey key, TValue value)
        {
            if (BlockReference.BlockId == 0)
            {
                BlockReference.BlockId = LeafNode<TKey, TValue>.CreateRoot(Config,
                                                                           new KeyValuePair<TKey, TValue>(key, value));
            }
            else
            {
                var blockId = BlockReference.BlockId;
                var root = Node<TKey, TValue>.GetNode(Config, BlockReference.BlockId);

                root.Insert(key, value,
                            added =>
                                {
                                    Config.Blocks.FreeBlock(blockId);
                                    if (added.Count == 1)
                                    {
                                        BlockReference.BlockId =
                                            added[0].Value;
                                    }
                                    else
                                    {
                                        BlockReference.BlockId = InteriorNode
                                            <TKey, TValue>.CreateRoot(
                                                Config,
                                                added
                                            );
                                    }
                                });
            }
        }

        public void Remove(TKey key, Func<KeyValuePair<TKey, TValue>, bool> filter)
        {
            if (BlockReference.BlockId != 0)
            {
                var blockId = BlockReference.BlockId;
                var root = Node<TKey, TValue>.GetNode(Config, BlockReference.BlockId);

                root.Remove(key, filter,
                            added =>
                                {
                                    Config.Blocks.FreeBlock(blockId);
                                    if (added.Count == 1)
                                    {
                                        BlockReference.BlockId =
                                            added[0].Value;
                                    }
                                    else
                                    {
                                        BlockReference.BlockId = InteriorNode
                                            <TKey, TValue>.CreateRoot(
                                                Config,
                                                added
                                            );
                                    }
                                });
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key)
        {
            if (BlockReference.BlockId == 0)
            {
                return Enumerable.Empty<KeyValuePair<TKey, TValue>>();
            }
            return Node<TKey, TValue>.GetNode(Config, BlockReference.BlockId).Find(key);
        }

        #endregion
    }
}