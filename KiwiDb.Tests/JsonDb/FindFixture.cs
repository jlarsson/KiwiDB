using System;
using System.Linq;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    [TestFixture]
    public class FindFixture : IsolatedDatabaseFixture
    {
        [Test]
        public void AllIndexed()
        {
            var coll = GetCollection();

            Func<int, int[]> getTags = i =>
                                           {
                                               if ((i == 0) || (i == 10) || (i == 20))
                                               {
                                                   return new[] {1, 2, 3};
                                               }
                                               return null;
                                           };

            var objects = (from i in Enumerable.Range(0, 10000)
                           select new
                                      {
                                          Key = i.ToString(),
                                          Value = i,
                                          Tags = getTags(i)
                                      }).ToArray();

            foreach (var o in objects)
            {
                coll.Update(o.Key, o);
            }

            coll.Indices.EnsureIndex("Tags");
            var objectsWithTag1 = coll.Find(new {Tags = 1}).Select(kv => kv.Key).OrderBy(s => s).ToArray();

            CollectionAssert.AreEqual(new[] {"0", "10", "20"}, objectsWithTag1);


            var a = coll.Find(new object()).ToArray();
            //foreach (var rec in coll.Find(new object()))
            //{
            //    Console.Out.WriteLine(rec.Value);

            //}
        }
    }
}