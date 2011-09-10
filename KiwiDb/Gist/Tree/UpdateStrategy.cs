namespace KiwiDb.Gist.Tree
{
    public static class UpdateStrategy<TKey, TValue>
    {
        static UpdateStrategy()
        {
            UniqueKey = new UniqueKey<TKey, TValue>();
            UpdateKey = new UpdateKey<TKey, TValue>();
            AppendKey = new AppendKey<TKey, TValue>();
        }

        public static IUpdateStrategy<TKey, TValue> UniqueKey { get; private set; }
        public static IUpdateStrategy<TKey, TValue> UpdateKey { get; private set; }
        public static IUpdateStrategy<TKey, TValue> AppendKey { get; private set; }
    }
}