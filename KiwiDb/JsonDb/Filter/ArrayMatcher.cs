using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class ArrayMatcher : Matcher
    {
        public ArrayMatcher(IJsonArray value)
        {
            Value = value;
        }

        public IJsonArray Value { get; set; }

        public override bool VisitArray(IJsonArray value)
        {
            return false;
        }
    }
}