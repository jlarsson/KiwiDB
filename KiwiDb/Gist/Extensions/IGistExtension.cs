using System.Collections.Generic;
using System.IO;
using KiwiDb.Gist.Extensions;

namespace KiwiDb.Gist.Tree
{
    public interface IGistExtension<TKey, TValue>
    {
        IGistLeafRecords<TKey, TValue> CreateLeafRecords(IEnumerable<KeyValuePair<TKey, TValue>> record);
        IGistLeafRecords<TKey, TValue> CreateLeafRecords(BinaryReader reader);

        IGistIndexRecords<TKey> CreateIndexRecords(IEnumerable<KeyValuePair<TKey, int>> record);
        IGistIndexRecords<TKey> CreateIndexRecords(BinaryReader reader);
    }
}