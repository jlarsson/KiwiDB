using System.Linq;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture]
    public class EnsureIndexFixture: IsolatedDatabaseFixture
    {
        [Test]
        public void Test()
        {
            var coll = GetCollection();
            Assert.AreEqual(0, coll.Indices.All.Count());

            Assert.IsTrue(coll.Indices.EnsureIndex("X"));

            Assert.AreEqual(1, coll.Indices.All.Count(), "Expected add index to show up in list");

            Assert.AreEqual("X", coll.Indices.All.First().Key, "Expected index to have correct name");
        }

        [Test]
        public void EnsureIndexWithNoChange()
        {
            var coll = GetCollection();
            Assert.IsTrue(coll.Indices.EnsureIndex("X"));
            Assert.IsFalse(coll.Indices.EnsureIndex("X"));
        }

        [Test]
        public void UpdateIndexToUniqueAndExpectDuplicateKeyWhenRebuildingIndex()
        {
            var coll = GetCollection();
            Assert.IsTrue(coll.Indices.EnsureIndex("X"));

            coll.Update("first", new {X = 1});
            coll.Update("second", new { X = 1 });

            Assert.That(
                () => coll.Indices.EnsureIndex("X", new IndexOptions(){IsUnique = true}),
                Throws.TypeOf<DuplicateKeyException>());
        }
    }
}