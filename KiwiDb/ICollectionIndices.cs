using System;
using System.Collections.Generic;
using KiwiDb.JsonDb;
using KiwiDb.JsonDb.Index;

namespace KiwiDb
{
    public interface ICollectionIndices
    {
        IEnumerable<KeyValuePair<string, IndexOptions>> All { get; }
        void VisitIndex(string memberPath, Action<KeyValuePair<IndexValue,string>> visitor);
        bool EnsureIndex(string memberPath, IndexOptions options);
        bool DropIndex(string memberPath);
    }
}