using System;
using System.Collections.Generic;
using System.IO;
using KiwiDb.Gist.Extensions;
using KiwiDb.Util;

namespace KiwiDb.JsonDb.Index
{
    public class GistIndexValueType : IOrderedGistType<IndexValue>, IComparer<IndexValue>
    {
        public GistIndexValueType()
        {
            BoolComparer = Comparer<bool>.Default;
            DateTimeComparer = Comparer<DateTime>.Default;
            IntegerComparer = Comparer<Int64>.Default;
            NumberComparer = Comparer<double>.Default;
            StringComparer = Comparer<string>.Default;
        }

        public IComparer<bool> BoolComparer { get; set; }
        public IComparer<DateTime> DateTimeComparer { get; set; }
        public IComparer<Int64> IntegerComparer { get; set; }
        public IComparer<double> NumberComparer { get; set; }
        public IComparer<string> StringComparer { get; set; }

        #region IComparer<IndexValue> Members

        public int Compare(IndexValue x, IndexValue y)
        {
            if (x.Type == y.Type)
            {
                switch (x.Type)
                {
                    case IndexValueType.Null:
                        return 0;
                    case IndexValueType.DateTime:
                        return DateTimeComparer.Compare((DateTime) x.Value, (DateTime) y.Value);
                    case IndexValueType.Integer:
                        return IntegerComparer.Compare((Int64) x.Value, (Int64) y.Value);
                    case IndexValueType.Number:
                        return NumberComparer.Compare((double) x.Value, (double) y.Value);
                    case IndexValueType.String:
                        return StringComparer.Compare((string) x.Value, (string) y.Value);
                    case IndexValueType.Bool:
                        return BoolComparer.Compare((bool) x.Value, (bool) y.Value);
                    default:
                        Verify.Argument(true, "Attempt to compare illegal values, {0} and {1}", x.Value, y.Value);
                        throw null;
                }
            }
            return Comparer<int>.Default.Compare((int) x.Type, (int) y.Type);
        }

        #endregion

        #region IOrderedGistType<IndexValue> Members

        public IComparer<IndexValue> Comparer
        {
            get { return this; }
        }

        public IndexValue Read(BinaryReader reader)
        {
            var type = (IndexValueType) reader.ReadByte();
            switch (type)
            {
                case IndexValueType.Null:
                    return new IndexValue();
                case IndexValueType.DateTime:
                    return new IndexValue(DateTime.FromBinary(reader.ReadInt64()));
                case IndexValueType.Integer:
                    return new IndexValue(reader.ReadInt64());
                case IndexValueType.Number:
                    return new IndexValue(reader.ReadDouble());
                case IndexValueType.String:
                    return new IndexValue(reader.ReadString());
                case IndexValueType.Bool:
                    return new IndexValue(reader.ReadBoolean());
                default:
                    Verify.Argument(true, "Illegal tag found in index");
                    throw null;
            }
        }

        public void Write(BinaryWriter writer, IndexValue value)
        {
            writer.Write((byte) value.Type);
            switch (value.Type)
            {
                case IndexValueType.Null:
                    break;
                case IndexValueType.DateTime:
                    writer.Write(((DateTime) value.Value).ToBinary());
                    break;
                case IndexValueType.Integer:
                    writer.Write((Int64) value.Value);
                    break;
                case IndexValueType.Number:
                    writer.Write((double) value.Value);
                    break;
                case IndexValueType.String:
                    writer.Write((string) value.Value);
                    break;
                case IndexValueType.Bool:
                    writer.Write((bool) value.Value);
                    break;
                default:
                    Verify.Argument(true, "Attempt to index illegal value {0}", value.Value);
                    throw null;
            }
        }

        #endregion
    }
}