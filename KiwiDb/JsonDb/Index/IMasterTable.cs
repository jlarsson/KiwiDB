using Kiwi.Json.Untyped;
using KiwiDb.Gist.Tree;

namespace KiwiDb.JsonDb.Index
{
    public interface IMasterTable : IIndex<string, IJsonValue>
    {
    }
}