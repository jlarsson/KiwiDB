using System.Collections.Generic;
using System.IO;

namespace KiwiDb.Gist.Extensions
{
    public class GistIntType : IOrderedGistType<int>
    {
        public static readonly GistIntType Default = new GistIntType();

        public GistIntType() : this(Comparer<int>.Default)
        {
        }

        public GistIntType(IComparer<int> comparer)
        {
            Comparer = comparer;
        }

        #region IOrderedGistType<int> Members

        public IComparer<int> Comparer { get; private set; }

        public int Read(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        public void Write(BinaryWriter writer, int value)
        {
            writer.Write(value);
        }

        #endregion
    }
}