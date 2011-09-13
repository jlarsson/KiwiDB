using System;
using System.Linq;

namespace KiwiDb.JsonDb
{
    public class IndexOptions: IEquatable<IndexOptions>
    {
        public bool IsUnique { get; set; }
        public bool WhenStringThenIgnoreCase { get; set; }
        public bool WhenDateThenIgnoreTimeOfDay { get; set; }

        public object[] IncludeValues { get; set; }
        public object[] ExcludeValues { get; set; }

        public bool Equals(IndexOptions other)
        {
            return !ReferenceEquals(null, other)
                   && IsUnique.Equals(other.IsUnique)
                   && WhenStringThenIgnoreCase.Equals(other.WhenStringThenIgnoreCase)
                   && WhenDateThenIgnoreTimeOfDay.Equals(other.WhenDateThenIgnoreTimeOfDay)
                   && (IncludeValues ?? Enumerable.Empty<object>()).SequenceEqual(other.IncludeValues ?? Enumerable.Empty<object>())
                   && (ExcludeValues ?? Enumerable.Empty<object>()).SequenceEqual(other.ExcludeValues ?? Enumerable.Empty<object>());
        }
    }
}