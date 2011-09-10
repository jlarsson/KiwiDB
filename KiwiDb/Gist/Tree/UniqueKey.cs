namespace KiwiDb.Gist.Tree
{
    public class UniqueKey<TKey, TValue> : IUpdateStrategy<TKey, TValue>
    {
        #region IUpdateStrategy<TKey,TValue> Members

        public void PrepareUpdate(TKey key, TValue value, IUpdateActions actions)
        {
            actions.FailIfKeyExists();
        }

        #endregion
    }
}