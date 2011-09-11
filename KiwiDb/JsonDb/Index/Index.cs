using System.Collections.Generic;
using System.Linq;
using KiwiDb.Gist.Tree;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb.Index
{
    public class Index : Gist<IndexValue, string>, IIndex
    {
        public Index(IBlockReference blockReference, IndexDefinition indexDefinition)
            : base(blockReference, indexDefinition.CreateGistConfig(blockReference.Blocks))
            //: base(blockReference, new GistConfig<IndexValue, string>
            //                           {
            //                               Blocks = blockReference.Blocks,
            //                               UpdateStrategy = UpdateStrategy<IndexValue, string>.AppendKey,
            //                               Ext =
            //                                   new OrderedGistExtension<IndexValue, string>(new GistIndexValueType(),
            //                                                                                new GistStringType())
            //                           })
        {
        }

        #region IIndex Members

        public string MemberPath { get; set; }


        public IEnumerable<string> FindKeys(IndexValue indexValue)
        {
            return Find(indexValue).Select(kv => kv.Value);
        }

        public void AddIndex(string key, IndexValue indexValue)
        {
            Insert(indexValue, key);
        }

        public void RemoveIndex(string key, IndexValue indexValue)
        {
            Remove(indexValue, kv => key.Equals(kv.Value));
        }

        #endregion
    }
}