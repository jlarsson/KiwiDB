namespace KiwiDb.Storage
{
    public interface IBlock
    {
        IBlockCollection BlockCollection { get; }
        byte[] Data { get; }
        int BlockId { get; }
    }
}