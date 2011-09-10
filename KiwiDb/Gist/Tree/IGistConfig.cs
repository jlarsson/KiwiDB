using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public interface IGistConfig<TKey, TValue>
    {
        IBlockCollection Blocks { get; }
        IGistExtension<TKey, TValue> Ext { get; }
        IUpdateStrategy<TKey, TValue> UpdateStrategy { get; }
    }
}