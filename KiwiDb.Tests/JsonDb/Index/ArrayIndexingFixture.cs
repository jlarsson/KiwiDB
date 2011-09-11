using System;
using System.Collections.Generic;
using System.Linq;
using KiwiDb.JsonDb;
using KiwiDb.JsonDb.Index;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture]
    public class ArrayIndexingFixture : IsolatedDatabaseFixture
    {
        [Test]
        public void AllValuesInArrayAreIndexed()
        {
            var coll = GetCollection();

            // Make sure property Tags is indexed
            coll.Indices.EnsureIndex("Tags");

            // Insert something where Tags is list of values
            const string key = "a key of some sort";
            var values = new HashSet<object> {"A", "B", DateTime.Now, null, Math.PI};
            coll.Update(key, new {Tags = values});

            // Now, extract what the index really contains
            var indexContent = new List<KeyValuePair<IndexValue, string>>();
            coll.Indices.VisitIndex("Tags", indexContent.Add);

            // First of all, each index entry must point to our key
            foreach (var kv in indexContent)
            {
                Assert.AreEqual(key, kv.Value);
            }

            // Second, the index should contain the array values
            var indexValues = new HashSet<object>(indexContent.Select(kv => kv.Key.Value));

            Assert.AreEqual(0, indexValues.Except(values).Count(), "Index values are too many");
            Assert.AreEqual(0, values.Except(indexValues).Count(), "Index values are too feew");
        }
    }
}