using System.Collections.Generic;
using System.Linq;

namespace KiwiDb.Storage
{
    public class BlockIdSet
    {
        private readonly HashSet<int> _blockIds = new HashSet<int>();

        public bool IsChanged { get; set; }

        public void Add(int blockId)
        {
            IsChanged |= _blockIds.Add(blockId);
        }

        public bool TryPop(out int blockId)
        {
            if (_blockIds.Count > 0)
            {
                blockId = _blockIds.First();
                _blockIds.Remove(blockId);
                IsChanged = true;
                return true;
            }
            blockId = default(int);
            return false;
        }

        public void AddTo(HashSet<int> set)
        {
            foreach (var blockId in _blockIds)
            {
                set.Add(blockId);
            }
        }
    }
}