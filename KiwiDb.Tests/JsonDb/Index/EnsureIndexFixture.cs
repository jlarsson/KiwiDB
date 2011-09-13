using System.Collections.Generic;
using System.Linq;
using Kiwi.Json.Untyped;
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

        [Test]
        public void IndexWithExclusionList()
        {
            var coll = GetCollection();
            Assert.IsTrue(coll.Indices.EnsureIndex("Value",
                                                   new IndexOptions() {ExcludeValues = new object[] {0, 1, 2, 3, 4}}));

            // Insert values 0..9 with keys "0".."9"
            foreach (var i in Enumerable.Range(0,10))
            {
                coll.Update(i.ToString(), new {Value = i});
            }

            // Fetch the keys in the index...
            var indexKeys = new List<string>();
            coll.Indices.VisitIndex("Value", v => indexKeys.Add(v.Value));

            // and ensure that the first 5 was skipped
            CollectionAssert.AreEqual(
                new []{"5","6","7","8","9"},
                indexKeys);
        }

        [Test]
        public void IndexWithInclusionList()
        {
            var coll = GetCollection();
            Assert.IsTrue(coll.Indices.EnsureIndex("Value",
                                                   new IndexOptions() { IncludeValues = new object[] { 0, 1, 2, 3, 4 } }));

            // Insert values 0..9 with keys "0".."9"
            foreach (var i in Enumerable.Range(0, 10))
            {
                coll.Update(i.ToString(), new { Value = i });
            }

            // Fetch the keys in the index...
            var indexKeys = new List<string>();
            coll.Indices.VisitIndex("Value", v => indexKeys.Add(v.Value));

            // and ensure that only the first 5 is there
            CollectionAssert.AreEqual(
                new[] { "0", "1", "2", "3", "4" },
                indexKeys);
        }
    }
}