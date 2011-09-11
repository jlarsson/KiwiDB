using System.Collections.Generic;
using System.Linq;
using Kiwi.Json.JPath;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Index
{
    public class IndexValueFactory : IIndexValueFactory
    {
        private readonly IndexValueVisitor _visitor = new IndexValueVisitor();

        #region IIndexValueFactory Members

        public IEnumerable<IndexValue> GetIndexValues(IJsonValue value, string memberPath)
        {
            var indexValue = new JsonPath(memberPath).GetValue(value);
            return indexValue == null ? Enumerable.Empty<IndexValue>() : GetIndexValues(indexValue);
        }

        #endregion

        protected IEnumerable<IndexValue> GetIndexValues(IJsonValue value)
        {
            return value.Visit(_visitor);
        }

        #region Nested type: IndexValueVisitor

        private class IndexValueVisitor : IJsonValueVisitor<IEnumerable<IndexValue>>
        {
            #region IJsonValueVisitor<IEnumerable<IndexValue>> Members

            public IEnumerable<IndexValue> VisitArray(IJsonArray value)
            {
                return from elem in value
                       from indexValue in elem.Visit(this)
                       select indexValue;
            }

            public IEnumerable<IndexValue> VisitBool(IJsonBool value)
            {
                yield return new IndexValue(value.Value);
            }

            public IEnumerable<IndexValue> VisitDate(IJsonDate value)
            {
                yield return new IndexValue(value.Value);
            }

            public IEnumerable<IndexValue> VisitDouble(IJsonDouble value)
            {
                yield return new IndexValue(value.Value);
            }

            public IEnumerable<IndexValue> VisitInteger(IJsonInteger value)
            {
                yield return new IndexValue(value.Value);
            }

            public IEnumerable<IndexValue> VisitNull(IJsonNull value)
            {
                yield return new IndexValue();
            }

            public IEnumerable<IndexValue> VisitObject(IJsonObject value)
            {
                yield break;
            }

            public IEnumerable<IndexValue> VisitString(IJsonString value)
            {
                yield return new IndexValue(value.Value);
            }

            #endregion
        }

        #endregion
    }
}