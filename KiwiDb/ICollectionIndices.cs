using System;
using System.Collections.Generic;
using KiwiDb.JsonDb;
using KiwiDb.JsonDb.Index;

namespace KiwiDb
{
    public interface ICollectionIndices
    {
        void VisitIndex(string memberPath, Action<KeyValuePair<IndexValue,string>> visitor);
        void EnsureIndex(string memberPath, IndexOptions options);
        bool DropIndex(string memberPath);
    }
}