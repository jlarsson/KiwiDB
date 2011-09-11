using System.Linq;
using KiwiDb.Gist.Extensions;
using KiwiDb.Util;

namespace KiwiDb.Gist.Tree
{
    public class UpdateActions<TKey, TValue> : IUpdateActions
    {
        private readonly TKey _key;
        private readonly IGistLeafRecords<TKey, TValue> _records;

        public UpdateActions(IGistLeafRecords<TKey, TValue> records, TKey key)
        {
            _records = records;
            _key = key;
        }

        #region IUpdateActions Members

        public void FailIfKeyExists()
        {
            if (_records.Find(_key).Take(1).Count() > 0)
            {
                throw Verify.DuplicateKey(_key);
            }
        }

        public void UpdateExistingKey()
        {
            _records.Remove(_key, kv => true);
        }

        public void AppendNewKey()
        {
            // Do nothing
        }

        #endregion
    }
}