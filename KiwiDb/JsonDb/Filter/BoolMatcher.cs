using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class BoolMatcher : Matcher
    {
        public BoolMatcher(IJsonBool value)
        {
            Value = value;
        }

        public IJsonBool Value { get; set; }

        public override bool VisitBool(IJsonBool value)
        {
            return (Value.Value == value.Value);
        }
    }
}