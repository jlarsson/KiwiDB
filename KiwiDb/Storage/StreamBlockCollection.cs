using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KiwiDb.Util;

namespace KiwiDb.Storage
{
    public class StreamBlockCollection : IBlockCollection, IBlockReference
    {
        public const int MasterBlockSize = 2*Magic.SignatureLength + 4*Size.Int;
        public static readonly string MagicSignature = "Kiwi DB database - licensed under MIT";

        private readonly HashSet<int> _allocatedBlocks = new HashSet<int>();
        private readonly Dictionary<int, ChainedBlock> _blocks = new Dictionary<int, ChainedBlock>();
        private readonly HashSet<int> _garbageBlocks = new HashSet<int>();
        private readonly Stream _stream;
        private HashSet<int> _freeBlocks;
        private HashSet<int> _initialFreeBlocks;
        private MasterBlockData _masterBlockData;

        private int? _tailBlockId;

        protected StreamBlockCollection(Stream stream)
        {
            _stream = stream;
            MasterBlockReference = new BlockReference(this, () => MasterBlockId, value => MasterBlockId = value);

            ApplicationBlockReference = new BlockReference(this, () => ApplicationBlockId,
                                                           value => ApplicationBlockId = value);
        }

        protected bool IsChanged { get; private set; }

        protected int TailBlockId
        {
            get
            {
                if (!_tailBlockId.HasValue)
                {
                    _tailBlockId = (int) ((_stream.Length + BlockSize - 1)/BlockSize);
                }
                return _tailBlockId.Value;
            }
            set { _tailBlockId = value; }
        }

        private MasterBlockData MasterBlockData
        {
            get { return _masterBlockData ?? (_masterBlockData = ReadMasterBlockData()); }
        }

        public HashSet<int> InitialFreeBlocks
        {
            get { return _initialFreeBlocks ?? (_initialFreeBlocks = ReadFreeBlocks()); }
        }


        public HashSet<int> FreeBlocks
        {
            get { return _freeBlocks ?? (_freeBlocks = new HashSet<int>(InitialFreeBlocks)); }
        }

        public HashSet<int> GarbageBlocks
        {
            get { return _garbageBlocks; }
        }

        public HashSet<int> AllocatedBlocks
        {
            get { return _allocatedBlocks; }
        }

        protected int MaxPayloadPerBlock
        {
            get { return BlockSize - Size.Int; }
        }

        public int BlockSize
        {
            get { return MasterBlockData.BlockSize; }
        }

        public int MasterBlockId
        {
            get { return MasterBlockData.MasterBlockId; }
            set
            {
                MasterBlockData.MasterBlockId = value;
                IsChanged = true;
            }
        }

        public int ApplicationBlockId
        {
            get { return MasterBlockData.ApplicationBlockId; }
            set
            {
                MasterBlockData.ApplicationBlockId = value;
                IsChanged = true;
            }
        }

        #region IBlockCollection Members

        public IBlockReference MasterBlockReference { get; private set; }

        public IBlockReference ApplicationBlockReference { get; private set; }

        public bool AutoCommit { get; set; }

        public bool IsLargeData(byte[] data)
        {
            return data.Length > MaxPayloadPerBlock;
        }

        public IBlock GetBlock(int blockId)
        {
            VerifyValidDataBlock(blockId);
            return new Block {BlockCollection = this, BlockId = blockId, Data = GetBlockData(blockId)};
        }

        public IBlock AllocateBlock(byte[] data)
        {
            // Calculate how many blocks are needed
            var blocksNeeded = (data.Length + MaxPayloadPerBlock - 1)/MaxPayloadPerBlock;

            var chainedBlock = new ChainedBlock
                                   {
                                       Data = data,
                                       BlockIds =
                                           Enumerable.Range(0, blocksNeeded).Select(i => AllocateBlockId()).ToArray()
                                   };
            _blocks.Add(chainedBlock.FirstBlockId, chainedBlock);
            return new Block
                       {
                           BlockCollection = this,
                           BlockId = chainedBlock.FirstBlockId,
                           Data = data
                       };
        }

        public void FreeBlock(int blockId)
        {
            VerifyValidDataBlock(blockId);
            IsChanged = true;

            var chainedBlock = _blocks[blockId];

            if (AllocatedBlocks.Contains(blockId) || InitialFreeBlocks.Contains(blockId))
            {
                foreach (var id in chainedBlock.BlockIds)
                {
                    FreeBlocks.Add(id);
                }
            }
            else
            {
                foreach (var id in chainedBlock.BlockIds)
                {
                    GarbageBlocks.Add(id);
                }
            }
            _blocks.Remove(blockId);
        }

        public void Dispose()
        {
            if (AutoCommit)
            {
                SaveChanges();
            }
            _stream.Dispose();
        }

        #endregion

        #region IBlockReference Members

        IBlockCollection IBlockReference.Blocks
        {
            get { return this; }
        }

        int IBlockReference.BlockId
        {
            get { return MasterBlockId; }
            set { MasterBlockId = value; }
        }

        #endregion

        private void SaveChanges()
        {
            if (!IsChanged)
            {
                return;
            }
            MasterBlockData.FreeListBlockId = WriteFreeBlocks();
            foreach (var kv in _blocks.Where(kv => AllocatedBlocks.Contains(kv.Key)))
            {
                WriteChainedBlock(kv.Value);
            }
            FlushStreamBuffers();
            WriteMasterBlockData();

            //Console.Out.WriteLine("Written: {0}, Free: {1}, Initial free: {2}, Allocated: {3}, Garbage: {4}",
            //                      _blocks.Count, FreeBlocks.Count, InitialFreeBlocks.Count, AllocatedBlocks.Count,
            //                      GarbageBlocks.Count);
        }

        protected virtual void FlushStreamBuffers()
        {
        }

        private void VerifyValidDataBlock(int blockId)
        {
            Verify.Argument(blockId > 0, "Invalid data block id: {0}", blockId);
        }

        private int AllocateBlockId()
        {
            var freeBlock = FreeBlocks.Where(b => !InitialFreeBlocks.Contains(b)).FirstOrDefault();
            if (freeBlock == 0)
            {
                freeBlock = FreeBlocks.FirstOrDefault();
            }
            if (freeBlock == 0)
            {
                freeBlock = TailBlockId++;
            }
            FreeBlocks.Remove(freeBlock);
            AllocatedBlocks.Add(freeBlock);
            return freeBlock;
        }

        private byte[] GetBlockData(int blockId)
        {
            ChainedBlock chainedBlock;
            if (!_blocks.TryGetValue(blockId, out chainedBlock))
            {
                chainedBlock = ReadChainedBlock(blockId);
                _blocks.Add(blockId, chainedBlock);
            }
            return chainedBlock.Data;
        }

        protected ChainedBlock ReadChainedBlock(int blockId)
        {
            var nextBlockId = blockId;
            var bytes = new MemoryStream();
            var blockIds = new List<int>();
            while (nextBlockId != 0)
            {
                blockIds.Add(nextBlockId);
                _stream.Position = nextBlockId*BlockSize;
                var data = new byte[BlockSize];
                _stream.Read(data, 0, data.Length);

                bytes.Write(data, Size.Int, data.Length - Size.Int);

                nextBlockId = BitConverter.ToInt32(data, 0);
            }
            return new ChainedBlock
                       {
                           Data = bytes.GetBuffer(),
                           BlockIds = blockIds.ToArray()
                       };
        }

        private void WriteChainedBlock(ChainedBlock chainedBlock)
        {
            var dataOffset = 0;
            for (var i = 0; i < chainedBlock.BlockIds.Length; ++i)
            {
                var blockId = chainedBlock.BlockIds[i];
                var nextBlockId = i < chainedBlock.BlockIds.Length - 1 ? chainedBlock.BlockIds[i + 1] : 0;

                _stream.Position = BlockSize*blockId;
                _stream.Write(BitConverter.GetBytes(nextBlockId), 0, Size.Int);

                var n = Math.Min(MaxPayloadPerBlock, chainedBlock.Data.Length - dataOffset);
                _stream.Write(chainedBlock.Data, dataOffset, n);

                dataOffset += MaxPayloadPerBlock;
            }
        }

        private HashSet<int> ReadFreeBlocks()
        {
            var blockId = MasterBlockData.FreeListBlockId;
            if (blockId == 0)
            {
                return new HashSet<int>();
            }

            var chainedBlock = ReadChainedBlock(blockId);
            var blocks = new HashSet<int>();

            using (var memoryStream = new MemoryStream(chainedBlock.Data, false))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; ++i)
                    {
                        blocks.Add(reader.ReadInt32());
                    }
                }
            }

            // The current blocks holding the free list must not be overwritten in this session
            foreach (var id in chainedBlock.BlockIds)
            {
                GarbageBlocks.Add(id);
                blocks.Remove(id);
            }

            return blocks;
        }

        private int WriteFreeBlocks()
        {
            var freeBlocks = new HashSet<int>(FreeBlocks.Concat(GarbageBlocks).Where(x => x != 0));

            if (freeBlocks.Count == 0)
            {
                return 0;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(freeBlocks.Count);
                    foreach (var freeBlock in freeBlocks)
                    {
                        writer.Write(freeBlock);
                    }
                    return AllocateBlock(memoryStream.ToArray()).BlockId;
                }
            }
        }

        protected static void InitializeEmptyCollection(Stream stream, int blockSize)
        {
            Verify.Argument(blockSize >= MasterBlockSize, "Invalid block size");
            var mbd = new MasterBlockData
                          {
                              MasterBlockId = 0,
                              BlockSize = blockSize,
                              FreeListBlockId = 0
                          };
            WriteMasterBlockData(stream, mbd);
        }

        private MasterBlockData ReadMasterBlockData()
        {
            _stream.Position = 0;
            var data = new byte[MasterBlockSize];
            _stream.Read(data, 0, data.Length);

            using (var stream = new MemoryStream(data, false))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var signature = Magic.Read(reader);
                    Verify.Argument(MagicSignature.Equals(signature), "Invalid database signature");
                    return new MasterBlockData
                               {
                                   ApplicationMagicSignature = Magic.Read(reader),
                                   BlockSize = reader.ReadInt32(),
                                   MasterBlockId = reader.ReadInt32(),
                                   ApplicationBlockId = reader.ReadInt32(),
                                   FreeListBlockId = reader.ReadInt32()
                               };
                }
            }
        }

        private void WriteMasterBlockData()
        {
            WriteMasterBlockData(_stream, MasterBlockData);
        }

        private static void WriteMasterBlockData(Stream stream, MasterBlockData mbd)
        {
            var data = new byte[mbd.BlockSize];
            using (var memoryStream = new MemoryStream(data, true))
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(Magic.MakeSignature(MagicSignature));
                    writer.Write(Magic.MakeSignature(mbd.ApplicationMagicSignature));
                    writer.Write(mbd.BlockSize);
                    writer.Write(mbd.MasterBlockId);
                    writer.Write(mbd.ApplicationBlockId);
                    writer.Write(mbd.FreeListBlockId);
                }
            }
            stream.Position = 0;
            stream.Write(data, 0, data.Length);
        }

        #region Nested type: BlockReference

        private class BlockReference : IBlockReference
        {
            public BlockReference(IBlockCollection blocks, Func<int> getter, Action<int> setter)
            {
                Getter = getter;
                Setter = setter;
                Blocks = blocks;
            }

            public Func<int> Getter { get; private set; }
            public Action<int> Setter { get; private set; }

            #region IBlockReference Members

            public IBlockCollection Blocks { get; private set; }

            public int BlockId
            {
                get { return Getter(); }
                set { Setter(value); }
            }

            #endregion
        }

        #endregion
    }
}