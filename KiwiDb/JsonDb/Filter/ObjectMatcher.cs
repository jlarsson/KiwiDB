using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class ObjectMatcher : Matcher
    {
        private readonly IJsonFilter _filter;

        public ObjectMatcher(IJsonFilter filter)
        {
            _filter = filter;
        }

        public override bool VisitObject(IJsonObject value)
        {
            return _filter.Matches(value);
        }
    }
}