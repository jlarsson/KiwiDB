using System.Collections.Generic;

namespace KiwiDb.Storage
{
    public interface IBlockCollectionStats
    {
        int RecordCount { get; }
        int IndexRecordCount { get; }
        int MaxBlockId { get; }
        IEnumerable<int> UsedBlocks { get; }
        IEnumerable<int> FreeBlocks { get; }
    }
}