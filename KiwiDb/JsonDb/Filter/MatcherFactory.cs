using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public class MatcherFactory : IJsonValueVisitor<Matcher>
    {
        #region IJsonValueVisitor<Matcher> Members

        public Matcher VisitArray(IJsonArray value)
        {
            return new ArrayMatcher(value, this);
        }


        public Matcher VisitBool(IJsonBool value)
        {
            return new BoolMatcher(value);
        }


        public Matcher VisitDate(IJsonDate value)
        {
            return new DateMatcher(value);
        }


        public Matcher VisitDouble(IJsonDouble value)
        {
            return new DoubleMatcher(value);
        }


        public Matcher VisitInteger(IJsonInteger value)
        {
            return new IntegerMatcher(value);
        }


        public Matcher VisitNull(IJsonNull value)
        {
            return new NullMatcher();
        }


        public Matcher VisitObject(IJsonObject value)
        {
            return new ObjectMatcher(new JsonFilter(value));
        }


        public Matcher VisitString(IJsonString value)
        {
            return new StringMatcher(value);
        }

        #endregion
    }
}