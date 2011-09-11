using System.IO;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    public class IsolatedDatabaseFixture
    {
        private readonly string _databasePath;

        public IsolatedDatabaseFixture()
        {
            _databasePath = Path.GetFullPath(@".\" + GetType().Name + ".kiwidb");
        }

        [SetUp]
        public void SetUp()
        {
            File.Delete(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
        }

        protected ICollection GetCollection()
        {
            return new Collection(_databasePath);
        }
    }
}