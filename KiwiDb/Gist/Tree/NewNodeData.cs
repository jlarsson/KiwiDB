using KiwiDb.Storage;

namespace KiwiDb.Gist.Tree
{
    public class NewNodeData<TKey>
    {
        public TKey MaxKey { get; set; }
        public IBlock Block { get; set; }
    }
}