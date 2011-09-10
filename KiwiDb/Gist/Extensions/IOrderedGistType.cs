using System.Collections.Generic;
using System.IO;

namespace KiwiDb.Gist.Extensions
{
    public interface IOrderedGistType<T>
    {
        IComparer<T> Comparer { get; }
        T Read(BinaryReader reader);
        void Write(BinaryWriter writer, T value);
    }
}