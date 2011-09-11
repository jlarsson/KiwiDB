using System;
using System.IO;

namespace KiwiDb.Storage
{
    public class FileStreamBlockCollection : StreamBlockCollection
    {
        private const int DefaultBlockSize = 4*1024;

        protected FileStreamBlockCollection(Stream stream) : base(stream)
        {
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
            var stream = CreateStream(databaseFileProvider, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new FileStreamBlockCollection(stream);
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
            var stream = CreateStream(databaseFileProvider, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return new FileStreamBlockCollection(stream);
        }

        private static FileStream CreateStream(IDatabaseFileProvider databaseFileProvider, FileMode mode, FileAccess access, FileShare share)
        {
            var endTime = DateTime.Now + databaseFileProvider.Timeout;
            while (true)
            {
                try
                {
                    return new FileStream(databaseFileProvider.Path, mode, access, share);
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
                    throw;
                }
            }
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