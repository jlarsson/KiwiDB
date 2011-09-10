using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class StringMatcher : Matcher
    {
        public StringMatcher(IJsonString value)
        {
            Value = value;
        }

        public IJsonString Value { get; set; }

        public override bool VisitString(IJsonString value)
        {
            return Value.Value == value.Value;
        }
    }
}