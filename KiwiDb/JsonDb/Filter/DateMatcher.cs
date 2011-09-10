using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class DateMatcher : Matcher
    {
        public DateMatcher(IJsonDate value)
        {
            Value = value;
        }

        public IJsonDate Value { get; set; }

        public override bool VisitDate(IJsonDate value)
        {
            return Value.Value == value.Value;
        }
    }
}