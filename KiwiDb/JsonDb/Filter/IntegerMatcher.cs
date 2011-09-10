using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class IntegerMatcher : Matcher
    {
        public IntegerMatcher(IJsonInteger value)
        {
            Value = value;
        }

        public IJsonInteger Value { get; set; }

        public override bool VisitInteger(IJsonInteger value)
        {
            return Value.Value == value.Value;
        }
    }
}