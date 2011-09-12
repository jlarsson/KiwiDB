using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    [TestFixture]
    public class RemoveFixture: IsolatedDatabaseFixture
    {
        [Test]
        public void RemoveAlsoClearsIndex()
        {
            var coll = GetCollection();

            coll.Update("k", new {X = 1});

            coll.Indices.EnsureIndex("X");

            var hasIndexValues = false;
            coll.Indices.VisitIndex("X", kv =>
                                             {
                                                 hasIndexValues = true;
                                                 Assert.AreEqual("k", kv.Value);
                                                 Assert.AreEqual(1, kv.Key.Value);
                                             });
            Assert.IsTrue(hasIndexValues);

            coll.Remove("k");

            coll.Indices.VisitIndex("X", kv => Assert.Fail("No index values should exists"));
        }
    }
}