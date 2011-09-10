using System;
using System.Linq;
using Kiwi.Json;
using KiwiDb.JsonDb.Index;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture]
    public class IndexValueFactoryFixture
    {
        [Test]
        public void ArrayValues()
        {
            var values = new object[] {1, Math.PI, "a string", new DateTime(2001, 08, 29)};
            var obj = JSON.FromObject(new {TheList = values});

            var indexValues = new IndexValueFactory().GetIndexValues(obj, "TheList").Select(v => v.Value).ToArray();

            CollectionAssert.AreEqual(values, indexValues);
        }
    }
}