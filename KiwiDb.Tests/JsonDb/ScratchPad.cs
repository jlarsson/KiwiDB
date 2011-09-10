using System.IO;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    [TestFixture]
    public class ScratchPad
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            File.Delete(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        private readonly string _databasePath;

        public ScratchPad()
        {
            _databasePath = Path.GetFullPath(@".\" + GetType().Name + ".kiwidb");
        }


        private Collection GetCollection()
        {
            return new Collection(_databasePath);
        }

        [Test]
        public void Test()
        {
            var coll = GetCollection();


            coll.EnsureIndex("Title");

            coll.Update("A", new {Title = "special", Text = "This is some text", Tags = new[] {"a", "b"}});
            coll.Update("B", new {Title = "another title", Text = "This is some text"});

            coll.EnsureIndex("Tags");

            var found = coll.Find(new {Tags = "a"});

            found = coll.Find(new {Tags = "a", Title = "x"});
            found = coll.Find(new {Tags = "a", Title = "special"});
        }
    }
}