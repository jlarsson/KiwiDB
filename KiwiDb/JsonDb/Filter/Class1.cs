using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class NullMatcher : Matcher
    {
        public override bool VisitNull(IJsonNull value)
        {
            return true;
        }
    }

    public class ObjectMatcher : Matcher
    {
        public ObjectMatcher(IJsonObject value)
        {
            Value = value;
        }

        public IJsonObject Value { get; set; }
    }
}