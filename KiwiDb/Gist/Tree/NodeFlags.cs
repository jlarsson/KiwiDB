using System;

namespace KiwiDb.Gist.Tree
{
    [Flags]
    public enum NodeFlags
    {
        IsLeafNode = 0x01,
        IsInteriorNode = 0x02
    }
}