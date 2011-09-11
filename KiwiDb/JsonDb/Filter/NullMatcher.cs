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
}