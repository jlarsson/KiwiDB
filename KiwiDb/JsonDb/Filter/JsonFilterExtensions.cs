using Kiwi.Json;

namespace KiwiDb.JsonDb.Filter
{
    public static class JsonFilterExtensions
    {
        public static bool Matches(this IJsonFilter filter, object value)
        {
            return filter.Matches(JSON.FromObject(value));
        }
    }
}