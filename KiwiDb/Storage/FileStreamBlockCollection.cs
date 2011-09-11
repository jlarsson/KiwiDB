using System;
using System.IO;
using System.Threading;
using KiwiDb.Util;

namespace KiwiDb.Storage
{
    public class FileStreamBlockCollection : StreamBlockCollection
    {
        private readonly IDisposable _lock;
        private static FairFileAccessScheduler Scheduler = new FairFileAccessScheduler();
        private const int DefaultBlockSize = 4*1024;

        protected FileStreamBlockCollection(Stream stream, IDisposable @lock) : base(stream)
        {
            _lock = @lock;
        }

        public override void Dispose()
        {
            base.Dispose();
            _lock.Dispose();
        }

        public static FileStreamBlockCollection CreateRead(string path)
        {
            return CreateRead(new DatabaseFileProvider()
                                  {
                                      Path = path,
                                      Timeout = TimeSpan.FromSeconds(30)
                                  });
        }

        public static FileStreamBlockCollection CreateRead(IDatabaseFileProvider databaseFileProvider)
        {
            return CreateCollection(databaseFileProvider, false, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static FileStreamBlockCollection CreateWrite(string path)
        {
            return CreateWrite(new DatabaseFileProvider()
            {
                Path = path,
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        public static FileStreamBlockCollection CreateWrite(IDatabaseFileProvider databaseFileProvider)
        {
            return CreateCollection(databaseFileProvider, true, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }

        private static FileStreamBlockCollection CreateCollection(IDatabaseFileProvider databaseFileProvider, bool writable, FileMode mode, FileAccess access, FileShare share)
        {
            var endTime = DateTime.Now + databaseFileProvider.Timeout;

            var @lock = writable ? 
                Scheduler.EnterWrite(databaseFileProvider.Path, databaseFileProvider.Timeout)
                : Scheduler.EnterRead(databaseFileProvider.Path, databaseFileProvider.Timeout);

            while (true)
            {
                try
                {
                    var stream = new FileStream(databaseFileProvider.Path, mode, access, share);
                    return new FileStreamBlockCollection(stream, @lock);
                }
                catch (FileNotFoundException)
                {
                    EnsureFile(databaseFileProvider.Path);
                }
                catch (IOException e)
                {
                    if (IoError.IsSharingViolation(e))
                    {
                        if (endTime > DateTime.Now)
                        {
                            databaseFileProvider.HandleSharingViolation();
                            continue;
                        }
                    }
                    @lock.Dispose();
                    throw;
                }
                catch(Exception)
                {
                    @lock.Dispose();
                    throw;
                }
            }
        }

        private static ReaderWriterLock GetLock(IDatabaseFileProvider databaseFileProvider)
        {
            
            throw new NotImplementedException();
        }

        private static void EnsureFile(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    InitializeEmptyCollection(stream, DefaultBlockSize);
                }
            }
            catch (IOException)
            {
                if (!File.Exists(path))
                {
                    throw;
                }
            }
        }
    }
}