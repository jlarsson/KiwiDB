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
            var stream = OpenSharedStream(TimeSpan.FromSeconds(30),
                                          () =>
                                          new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                                          () => EnsureFile(path));
            return new FileStreamBlockCollection(stream);
        }

        public static FileStreamBlockCollection CreateWrite(string path)
        {
            var stream = OpenSharedStream(TimeSpan.FromSeconds(30),
                                          () =>
                                          new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None),
                                          () => EnsureFile(path));
            return new FileStreamBlockCollection(stream);
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

        private static Stream OpenSharedStream(TimeSpan timeout, Func<Stream> opener, Action createIfMissing)
        {
            var endTime = DateTime.Now + timeout;
            while (true)
            {
                try
                {
                    return opener();
                }
                catch (FileNotFoundException e)
                {
                    createIfMissing();
                }
                catch (IOException e)
                {
                    if (IoError.IsSharingViolation(e))
                    {
                        if (endTime > DateTime.Now)
                        {
                            continue;
                        }
                    }
                    throw;
                }
            }
        }
    }
}