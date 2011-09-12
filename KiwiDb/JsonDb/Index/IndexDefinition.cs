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

        public IGistConfig<IndexValue, string> CreateGistConfig(IBlockCollection blocks)
        {
            return
                new GistConfig<IndexValue, string>
                    {
                        Blocks = blocks,
                        Ext =
                            new OrderedGistExtension<IndexValue, string>(
                            new GistIndexValueType
                                {
                                    StringComparer =
                                        Options.WhenStringThenIgnoreCase
                                            ? StringComparer.OrdinalIgnoreCase
                                            : StringComparer.Ordinal,
                                    DateTimeComparer =
                                        Options.WhenDateThenIgnoreTimeOfDay
                                            ? (IComparer<DateTime>) new IgnoreTimeOfDayComparer()
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