using System.Collections.Generic;
using System.Linq;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture]
    public class FilterIndexValuesFixture: IsolatedDatabaseFixture
    {
        object GetObject()
        {
            return 1;
        }
        [Test]
        public void Test()
        {
            object a = GetObject();
            var s = new HashSet<object>(new object[] {null, 1, "a"});
            var b = s.Contains(a);
            //Console.Out.WriteLine(b);


            // values to insert in collection
            var values = Enumerable.Range(0, 100).ToArray();

            // every third or so should be excluded from indexing (and thus Find())
            var noindexValues = values.Where(i => (i%3) == 0).ToArray();
            var indexedValues = values.Except(noindexValues).ToArray();

            var coll = GetCollection();

            // Setup index
            coll.Indices.EnsureIndex("A", new IndexOptions()
                                              {
                                                  ExcludeValues = noindexValues.Cast<object>().ToArray()
                                              });

            // Insert values
            foreach (var v in values)
            {
                coll.Update(v.ToString(), new { A = v });
            }

            // Ensure indexed are fetched ok
            Assert.IsTrue(noindexValues.Select(v => coll.Find(new { A = v })).All(l => l.Count() == 0));
            Assert.IsTrue(indexedValues.Select(v => coll.Find(new{A = v})).All(l => l.Count() == 1));

            
        }
        
    }
}