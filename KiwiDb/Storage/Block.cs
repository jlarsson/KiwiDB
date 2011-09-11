namespace KiwiDb.Storage
{
    public class Block : IBlock
    {
        #region IBlock Members

        public IBlockCollection BlockCollection { get; set; }
        public byte[] Data { get; set; }
        public int BlockId { get; set; }

        public object UserData
        {
            get { return BlockUserDataCollection.GetValue(BlockId); }
            set { BlockUserDataCollection.SetValue(BlockId, value); }
        }

        #endregion

        public IBlockUserDataCollection BlockUserDataCollection { get; set; }
    }

    public interface IBlockUserDataCollection
    {
        object GetValue(int blockId);
        void SetValue(int blockId, object value);
    }
}