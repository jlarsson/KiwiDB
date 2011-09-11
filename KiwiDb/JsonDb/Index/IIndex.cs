using System;
using System.Collections.Generic;

namespace KiwiDb.JsonDb.Index
{
    public interface IIndex
    {
        string MemberPath { get; }
        void Visit(Action<KeyValuePair<IndexValue, string>> visitor);
        IEnumerable<string> FindKeys(IndexValue indexValue);
        void AddIndex(string key, IndexValue indexValue);
        void RemoveIndex(string key, IndexValue indexValue);
        void Drop();
    }
}