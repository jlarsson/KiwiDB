using System.Collections.Generic;

namespace KiwiDb.Gist.Extensions
{
    public interface IGistIndexRecords<TKey> : IGistRecords<TKey, int>
    {
        IEnumerable<KeyValuePair<TKey, int>> GetRange(TKey key);
        KeyValuePair<TKey, int> RemoveInsertRecord(TKey key);
    }
}