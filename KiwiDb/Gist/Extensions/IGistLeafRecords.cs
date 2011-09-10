using System.Collections.Generic;

namespace KiwiDb.Gist.Extensions
{
    public interface IGistLeafRecords<TKey, TValue> : IGistRecords<TKey, TValue>
    {
        IEnumerable<KeyValuePair<TKey, TValue>> Find(TKey key);
    }
}