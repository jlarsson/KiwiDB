using System.Linq;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class ArrayMatcher : Matcher
    {
        private readonly MatcherFactory _matcherFactory;
        public ArrayMatcher(IJsonArray value, MatcherFactory matcherFactory)
        {
            _matcherFactory = matcherFactory;
            Value = value;
        }

        public IJsonArray Value { get; set; }

        public override bool VisitArray(IJsonArray value)
        {
            return (Value.Count == value.Count)
                   && Value.Zip(value, (a, b) => b.Visit(a.Visit(_matcherFactory))).All(b => b);
        }
    }
}