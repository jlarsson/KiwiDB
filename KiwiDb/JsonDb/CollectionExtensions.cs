using System.Collections.Generic;
using System.Linq;
using Kiwi.Json;
using Kiwi.Json.Untyped;

namespace KiwiDb.JsonDb
{
    public static class CollectionExtensions
    {
        public static T Get<T>(this ICollection collection, string key) where T : class
        {
            var obj = collection.Get(key);
            return obj == null ? default(T) : JSON.ToObject<T>(obj);
        }

        public static T Remove<T>(this ICollection collection, string key) where T : class
        {
            var obj = collection.Remove(key);
            return obj == null ? default(T) : JSON.ToObject<T>(obj);
        }

        public static IEnumerable<KeyValuePair<string, T>> Find<T>(this ICollection collection, object filter)
            where T : class
        {
            return from kv in collection.Find(JSON.FromObject(filter))
                   select new KeyValuePair<string, T>(kv.Key, JSON.ToObject<T>(kv.Value));
        }

        public static IEnumerable<KeyValuePair<string, IJsonValue>> Find(this ICollection collection, object filter)
        {
            return collection.Find(JSON.FromObject(filter));
        }

        public static T Update<T>(this ICollection collection, string key, T value) where T : class
        {
            var obj = collection.Update(key, JSON.FromObject(value));
            return obj == null ? default(T) : JSON.ToObject<T>(obj);
        }
    }
}