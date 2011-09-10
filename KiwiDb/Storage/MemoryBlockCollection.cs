using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KiwiDb.Storage
{
    public class MemoryBlockCollection : IBlockCollection, IBlockReference
    {
        private readonly Dictionary<int, byte[]> _blocks = new Dictionary<int, byte[]>();
        private int _nextBlockId = 1;

        public MemoryBlockCollection(int blockSize)
        {
            BlockSize = blockSize;
        }

        public int BlockSize { get; private set; }
        public int MasterBlockId { get; set; }

        #region IBlockCollection Members

        public IBlockReference MasterBlockReference
        {
            get { return this; }
        }

        public IBlockReference ApplicationBlockReference
        {
            get { throw new NotImplementedException(); }
        }

        public bool AutoCommit { get; set; }

        public bool IsLargeData(byte[] data)
        {
            return false;
        }

        public IBlock GetBlock(int blockId)
        {
            return new Block
                       {
                           BlockCollection = this,
                           BlockId = blockId,
                           Data = _blocks[blockId]
                       };
        }

        public IBlock AllocateBlock(byte[] data)
        {
            var blockId = _nextBlockId++;
            _blocks.Add(blockId, data);
            return GetBlock(blockId);
        }

        public void FreeBlock(int blockId)
        {
            Debug.Assert(_blocks.ContainsKey(blockId));
            _blocks.Remove(blockId);
        }

        public void Dispose()
        {
        }

        #endregion

        #region IBlockReference Members

        public IBlockCollection Blocks
        {
            get { return this; }
        }

        int IBlockReference.BlockId
        {
            get { return MasterBlockId; }
            set { MasterBlockId = value; }
        }

        #endregion
    }
}