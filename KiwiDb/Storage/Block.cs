namespace KiwiDb.Storage
{
    public class Block : IBlock
    {
        #region IBlock Members

        public IBlockCollection BlockCollection { get; set; }
        public byte[] Data { get; set; }
        public int BlockId { get; set; }

        #endregion
    }
}