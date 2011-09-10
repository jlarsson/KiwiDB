namespace KiwiDb.Gist.Tree
{
    public interface IUpdateStrategy<in TKey, in TValue>
    {
        void PrepareUpdate(TKey key, TValue value, IUpdateActions actions);
    }
}