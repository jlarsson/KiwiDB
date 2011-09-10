using System;

namespace KiwiDb.Storage
{
    public interface IBlockCollection : IDisposable
    {
        IBlockReference MasterBlockReference { get; }
        IBlockReference ApplicationBlockReference { get; }
        bool AutoCommit { get; set; }
        bool IsLargeData(byte[] data);
        IBlock GetBlock(int blockId);
        IBlock AllocateBlock(byte[] data);
        void FreeBlock(int blockId);
    }
}