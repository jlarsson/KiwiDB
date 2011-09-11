using System;
using System.IO;
using KiwiDb.JsonDb;
using KiwiDb.Storage;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    public class IsolatedDatabaseFixture
    {
        public DatabaseFileProvider DatabaseFileProvider { get; private set; }

        public IsolatedDatabaseFixture()
        {
            DatabaseFileProvider = new DatabaseFileProvider()
                                       {
                                           Path = Path.GetFullPath(@".\" + GetType().Name + ".kiwidb"),
                                           Timeout = TimeSpan.FromSeconds(2)
                                       };
        }

        [SetUp]
        public void SetUp()
        {
            File.Delete(DatabaseFileProvider.Path);
        }

        [TearDown]
        public void TearDown()
        {
        }

        protected ICollection GetCollection()
        {
            return new Collection(DatabaseFileProvider);
        }
    }
}