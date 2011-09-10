namespace KiwiDb.Storage
{
    public class ChainedBlock
    {
        public int FirstBlockId
        {
            get { return BlockIds[0]; }
        }

        public int[] BlockIds { get; set; }
        public byte[] Data { get; set; }
    }
}