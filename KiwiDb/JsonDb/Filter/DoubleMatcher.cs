using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class DoubleMatcher : Matcher
    {
        public DoubleMatcher(IJsonDouble value)
        {
            Value = value;
        }

        public IJsonDouble Value { get; set; }

        public override bool VisitDouble(IJsonDouble value)
        {
            return Value.Value == value.Value;
        }
    }
}