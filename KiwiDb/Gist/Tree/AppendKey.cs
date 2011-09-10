namespace KiwiDb.Gist.Tree
{
    public class AppendKey<TKey, TValue> : IUpdateStrategy<TKey, TValue>
    {
        #region IUpdateStrategy<TKey,TValue> Members

        public void PrepareUpdate(TKey key, TValue value, IUpdateActions actions)
        {
            actions.AppendNewKey();
        }

        #endregion
    }
}