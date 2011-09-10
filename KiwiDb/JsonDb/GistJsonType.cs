using System;
using System.Collections.Generic;
using System.IO;
using Kiwi.Json;
using Kiwi.Json.Untyped;
using KiwiDb.Gist.Extensions;

namespace KiwiDb.JsonDb
{
    public class GistJsonType : IOrderedGistType<IJsonValue>
    {
        #region IOrderedGistType<IJsonValue> Members

        public IComparer<IJsonValue> Comparer
        {
            get { throw new NotImplementedException(); }
        }

        public IJsonValue Read(BinaryReader reader)
        {
            return JSON.ReadBinary(reader);
        }

        public void Write(BinaryWriter writer, IJsonValue value)
        {
            JSON.WriteBinary(value, writer);
        }

        #endregion
    }
}