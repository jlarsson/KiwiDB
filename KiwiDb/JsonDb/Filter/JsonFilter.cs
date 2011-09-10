using System;
using System.Collections.Generic;
using System.Linq;
using Kiwi.Json.JPath;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class JsonFilter : IJsonFilter
    {
        private readonly IEnumerable<Tuple<IJsonPath, Matcher>> _matchers;

        public JsonFilter(IEnumerable<IJsonPathValue> pathValues)
        {
            var f = new MatcherFactory();
            _matchers = (from pathValue in pathValues
                         select Tuple.Create(pathValue.Path, pathValue.Value.Visit(f)))
                .ToList();
        }

        public JsonFilter(IJsonValue filter) : this(filter.JsonPathValues())
        {
        }

        #region IJsonFilter Members

        public bool Matches(IJsonValue value)
        {
            return _matchers.All(m =>
                                     {
                                         var child = m.Item1.GetValue(value);
                                         return (child != null) && child.Visit(m.Item2);
                                     });
        }

        #endregion
    }
}