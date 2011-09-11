using System;

namespace KiwiDb.Storage
{
    public interface IDatabaseFileProvider
    {
        string Path { get; }
        TimeSpan Timeout { get; }
        void HandleSharingViolation();
    }
}