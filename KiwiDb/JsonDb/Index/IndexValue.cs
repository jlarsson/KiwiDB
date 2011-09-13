using System;

namespace KiwiDb.JsonDb.Index
{
    public struct IndexValue
    {
        private IndexValue(IndexValueType type, object value)
            : this()
        {
            Type = type;
            Value = value;
        }

        public IndexValue(DateTime value) : this(IndexValueType.DateTime, value)
        {
        }

        public IndexValue(double value)
            : this(IndexValueType.Number, value)
        {
        }

        public IndexValue(int value)
            : this(IndexValueType.Integer, value)
        {
        }

        public IndexValue(string value) : this(IndexValueType.String, value)
        {
        }

        public IndexValue(bool value) : this(IndexValueType.Bool, value)
        {
        }

        public IndexValueType Type { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}({1})", Type, Value);
        }
    }
}