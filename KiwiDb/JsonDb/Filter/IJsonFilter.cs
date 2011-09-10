using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb.Filter
{
    public interface IJsonFilter
    {
        bool Matches(IJsonValue value);
    }
}