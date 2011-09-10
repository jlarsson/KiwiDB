using System.Collections.Generic;
using System.IO;

namespace KiwiDb.Gist.Extensions
{
    public class GistStringType : IOrderedGistType<string>
    {
        public GistStringType() : this(Comparer<string>.Default)
        {
        }

        public GistStringType(IComparer<string> comparer)
        {
            Comparer = comparer;
        }

        #region IOrderedGistType<string> Members

        public IComparer<string> Comparer { get; private set; }

        public string Read(BinaryReader reader)
        {
            return reader.ReadString();
        }

        public void Write(BinaryWriter writer, string value)
        {
            writer.Write(value);
        }

        #endregion
    }
}