using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace KiwiDb.Util
{
    public class FairFileAccessScheduler
    {
        private readonly object _sync = new object();
        private Dictionary<string, ReaderWriterLock> _locks = new Dictionary<string, ReaderWriterLock>();

        public IDisposable EnterRead(string path, TimeSpan timeout)
        {
            var @lock = GetLock(path);
            @lock.AcquireReaderLock(timeout);
            return new ReadLockHolder(@lock);
        }

        public IDisposable EnterWrite(string path, TimeSpan timeout)
        {
            var @lock = GetLock(path);
            @lock.AcquireWriterLock(timeout);
            return new WriteLockHolder(@lock);
        }

        private ReaderWriterLock GetLock(string path)
        {
            var key = Path.GetFullPath(path).ToLower();

            ReaderWriterLock @lock;
            if (_locks.TryGetValue(key, out @lock))
            {
                return @lock;
            }

            lock (_sync)
            {
                if (_locks.TryGetValue(key, out @lock))
                {
                    return @lock;
                }
                @lock = new ReaderWriterLock();
                _locks = new Dictionary<string, ReaderWriterLock>(_locks) {{key, @lock}};
                return @lock;
            }
        }

        #region Nested type: ReadLockHolder

        public class ReadLockHolder : IDisposable
        {
            private readonly ReaderWriterLock _lock;

            public ReadLockHolder(ReaderWriterLock @lock)
            {
                _lock = @lock;
            }

            #region IDisposable Members

            public void Dispose()
            {
                _lock.ReleaseReaderLock();
            }

            #endregion
        }

        #endregion

        #region Nested type: WriteLockHolder

        public class WriteLockHolder : IDisposable
        {
            private readonly ReaderWriterLock _lock;

            public WriteLockHolder(ReaderWriterLock @lock)
            {
                _lock = @lock;
            }

            #region IDisposable Members

            public void Dispose()
            {
                _lock.ReleaseWriterLock();
            }

            #endregion
        }

        #endregion
    }
}