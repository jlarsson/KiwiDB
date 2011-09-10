using KiwiDb.Gist.Extensions;
using KiwiDb.Gist.Tree;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb.Index
{
    public class IndexDefinition
    {
        public string Path { get; set; }
        public bool Unique { get; set; }
        public int BlockId { get; set; }

        public Gist<IndexValue, IndexValue> CreateIndex(IBlockCollection blocks)
        {
            return new Gist<IndexValue, IndexValue>(
                null,
                new GistConfig<IndexValue, IndexValue>
                    {
                        Blocks = blocks,
                        Ext =
                            new OrderedGistExtension<IndexValue, IndexValue>(
                            new GistIndexValueType(), new GistIndexValueType()),
                        UpdateStrategy = UpdateStrategy<IndexValue, IndexValue>.AppendKey
                    });
        }
    }
}