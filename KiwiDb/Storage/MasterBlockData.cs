namespace KiwiDb.Storage
{
    public class MasterBlockData
    {
        public string ApplicationMagicSignature { get; set; }
        public int BlockSize { get; set; }
        public int MasterBlockId { get; set; }
        public int ApplicationBlockId { get; set; }
        public int FreeListBlockId { get; set; }
    }
}