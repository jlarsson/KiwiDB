using KiwiDb.JsonDb;

namespace KiwiDb
{
    public static class CollectionIndicesExtensions
    {
        public static bool EnsureIndex(this ICollectionIndices indices, string memberPath)
        {
            return indices.EnsureIndex(memberPath, new IndexOptions());
        }
    }
}