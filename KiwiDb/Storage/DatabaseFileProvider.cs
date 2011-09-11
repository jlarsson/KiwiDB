using System;
using System.Threading;

namespace KiwiDb.Storage
{
    public class DatabaseFileProvider : IDatabaseFileProvider
    {
        public string Path { get; set; }

        public TimeSpan Timeout { get; set; }

        public void HandleSharingViolation()
        {
            Thread.Sleep(0);
        }
    }
}