using System;
using Kiwi.Json;
using KiwiDb.JsonDb.Filter;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Filter
{
    [TestFixture]
    public class JsonFilterFixture
    {
        private void TestFilter<T>(T hitValue, T missValue)
        {
            var filter = new JsonFilter(JSON.FromObject(new {TestField = hitValue}));
            Assert.IsTrue(filter.Matches(new {TestField = hitValue, Ignored = "whatever"}),
                          "Expected hit on exact value match");
            Assert.IsFalse(filter.Matches(new {TestField = missValue, Ignored = "who cares"}),
                           "Expected miss on exact value mismatch");

            Assert.IsTrue(filter.Matches(new {TestField = new[] {hitValue, missValue}, Ignored = "whatever"}),
                          "Expected hit on exact value in array");
            Assert.IsFalse(filter.Matches(new {TestField = new[] {missValue, missValue}, Ignored = "who cares"}),
                           "Expected miss on exact value not in array");
        }

        [Test]
        public void MatchArray()
        {
            var obj = new
                        {
                            F = new object[]
                                    {
                                        new {A = 1},
                                        2,
                                        "Three"
                                    }
                        };
            var superObj = new
                        {
                            F = new object[]
                                    {
                                        new
                                            {
                                                A = 1,
                                                Extra1 = 1
                                            },
                                        2,
                                        "Three"
                                    },
                            Extra = 1
                        };

            var badObj = new
                             {
                                 F = new object[]{1,2,3}
                             };

            var filter = new JsonFilter(JSON.FromObject(obj));

            Assert.IsTrue(filter.Matches(superObj));
            Assert.IsFalse(filter.Matches(badObj));
        }

        [Test]
        public void MatchBool()
        {
            TestFilter(true, false);
        }

        [Test]
        public void MatchDate()
        {
            TestFilter(DateTime.Now, DateTime.Now.AddDays(1));
        }

        [Test]
        public void MatchDouble()
        {
            TestFilter(Math.PI, Math.E);
        }


        [Test]
        public void MatchInteger()
        {
            TestFilter(10, 20);
        }

        [Test]
        public void MatchNull()
        {
            TestFilter(null, "not null");
        }

        //[Test]
        //public void MatchObject()
        //{
        //    TestFilter(new { A = 1, B = 2 }, new { A = 0, B = 0 });
        //}

        [Test]
        public void MatchString()
        {
            TestFilter("hello", "world");

            TestFilter("case matters", "CASE MATTERS");
        }
    }
}