using Kiwi.Json.Untyped;
using KiwiDb.Gist.Extensions;
using KiwiDb.Gist.Tree;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb.Index
{
    public class MasterTable : Gist<string, IJsonValue>, IMasterTable
    {
        public MasterTable(IBlockCollection blocks)
            : base(blocks.MasterBlockReference,
                   new GistConfig<string, IJsonValue>
                       {
                           Blocks = blocks,
                           Ext =
                               new OrderedGistExtension<string, IJsonValue>(
                               new GistStringType(), new GistJsonType()),
                           UpdateStrategy =
                               UpdateStrategy<string, IJsonValue>.UpdateKey
                       })
        {
        }
    }
}