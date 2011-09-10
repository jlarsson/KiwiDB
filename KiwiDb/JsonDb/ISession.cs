using System;
using KiwiDb.JsonDb.Index;

namespace KiwiDb.JsonDb
{
    public interface ISession : IDisposable
    {
        bool AutoCommit { get; set; }
        IMasterTable MasterTable { get; }
        IIndexCatalog IndexCatalog { get; }
    }
}