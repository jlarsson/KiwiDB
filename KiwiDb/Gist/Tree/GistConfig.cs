using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public class GistConfig<TKey, TValue> : IGistConfig<TKey, TValue>
    {
        #region IGistConfig<TKey,TValue> Members

        public IBlockCollection Blocks { get; set; }
        public IGistExtension<TKey, TValue> Ext { get; set; }

        public IUpdateStrategy<TKey, TValue> UpdateStrategy { get; set; }

        #endregion
    }
}