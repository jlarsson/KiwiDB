using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class ObjectMatcher : Matcher
    {
        public ObjectMatcher(IJsonObject value)
        {
            Value = value;
        }

        public IJsonObject Value { get; set; }
    }
}