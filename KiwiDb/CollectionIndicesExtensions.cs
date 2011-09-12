using KiwiDb.JsonDb;

namespace KiwiDb
{
    public static class CollectionIndicesExtensions
    {
        public static void EnsureIndex(this ICollectionIndices indices, string memberPath)
        {
            indices.EnsureIndex(memberPath, new IndexOptions());
        }
    }
}