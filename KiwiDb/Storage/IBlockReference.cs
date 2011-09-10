namespace KiwiDb.Storage
{
    public interface IBlockReference
    {
        IBlockCollection Blocks { get; }
        int BlockId { get; set; }
    }
}