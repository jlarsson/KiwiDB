namespace KiwiDb.JsonDb
{
    public interface ICollectionIndices
    {
        void EnsureIndex(string memberPath, IndexOptions options);
        bool DropIndex(string memberPath);
    }
}