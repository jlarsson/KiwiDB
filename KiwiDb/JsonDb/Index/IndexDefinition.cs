using System;
using System.Collections.Generic;
using KiwiDb.Gist.Extensions;
using KiwiDb.Gist.Tree;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb.Index
{
    public class IndexDefinition
    {
        public string Path { get; set; }
        public IndexOptions Options { get; set; }
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
                            new GistIndexValueType()
                                {
                                    StringComparer = Options.StringIndexOptions.IgnoreCase ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
                                }
                            , new GistIndexValueType()),
                        //UpdateStrategy = UpdateStrategy<IndexValue, IndexValue>.AppendKey
                        UpdateStrategy = Options.IsUnique ? UpdateStrategy<IndexValue, IndexValue>.UniqueKey : UpdateStrategy<IndexValue, IndexValue>.AppendKey
                    });
        }

        public IGistConfig<IndexValue, string> CreateGistConfig(IBlockCollection blocks)
        {
            return
                new GistConfig<IndexValue, string>
                    {
                        Blocks = blocks,
                        Ext =
                            new OrderedGistExtension<IndexValue, string>(
                            new GistIndexValueType()
                                {
                                    StringComparer =
                                        Options.StringIndexOptions.IgnoreCase
                                            ? StringComparer.OrdinalIgnoreCase
                                            : StringComparer.Ordinal,

                                    DateTimeComparer = 
                                        Options.DateIndexOptions.IgnoreTimeOfDay ?
                                            (IComparer<DateTime>)new IgnoreTimeOfDayComparer() 
                                            : Comparer<DateTime>.Default
                                }
                            , new GistStringType()),
                        UpdateStrategy =
                            Options.IsUnique
                                ? UpdateStrategy<IndexValue, string>.UniqueKey
                                : UpdateStrategy<IndexValue, string>.AppendKey
                    };
        }
    }
}