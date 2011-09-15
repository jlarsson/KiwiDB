using System;
using System.Collections.Generic;
using System.Linq;
using Kiwi.Json;
using KiwiDb;

namespace PerfTest
{
    internal class Program
    {
        private const string LoremIpsum =
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec feugiat pharetra dignissim. Duis feugiat dui ut tellus aliquam consectetur. Ut a pulvinar lorem. Pellentesque at elit in neque faucibus condimentum ac nec justo. Integer tincidunt eleifend justo. Nulla sed metus a est tempor aliquam et in lacus. Donec imperdiet cursus elit vitae sodales. Suspendisse lobortis condimentum lorem vitae volutpat. Donec scelerisque condimentum vestibulum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam sollicitudin erat vel tortor consequat eu convallis neque posuere. Aenean tortor eros, ultrices a vulputate sed, mollis at neque. Donec mollis scelerisque mi, at porta dui adipiscing et.";

        private static void Main(string[] args)
        {
            var tests = new[]
                            {
                                new KiwiPerformanceTest
                                    {
                                        Description = "Add new posts in same batch",
                                        Test = BatchInsert
                                    },
                                new KiwiPerformanceTest
                                    {
                                        Description = "Add new, indexed posts in same batch",
                                        Test = BatchInsertWithIndex
                                    },
                                new KiwiPerformanceTest
                                    {
                                        Description = "Add new posts individually",
                                        Test = Insert
                                    },
                                new KiwiPerformanceTest
                                    {
                                        Description = "Add new, indexed posts individually",
                                        Test = InsertWithIndex
                                    }
                            };

            const int warmupCount = 500;
            Console.Out.WriteLine("/testrunner: warming up with {0} iterations", warmupCount);
            foreach (var test in tests)
            {
                test.Run(warmupCount, new TestLog(test));
            }

            Console.Out.WriteLine();
            const int sampleCount = 5000;
            Console.Out.WriteLine("/testrunner: running tests with {0} iterations", sampleCount);
            foreach (var test in tests)
            {
                test.Run(sampleCount, new TestLog(test));
            }
        }

        private static int InsertWithIndex(ICollection coll, int n)
        {
            coll.Indices.EnsureIndex("UserId");
            return Insert(coll, n);
        }

        private static int Insert(ICollection coll, int n)
        {
            var count = 0;
            foreach (var post in GetTestData(n))
            {
                ++count;
                coll.Update(post.DataId, JSON.FromObject(post));
            }
            return count;
        }

        private static int BatchInsertWithIndex(ICollection coll, int n)
        {
            coll.Indices.EnsureIndex("UserId");
            return BatchInsert(coll, n);
        }

        private static IEnumerable<TestModelData> GetTestData(int count)
        {
            //var text = string.Join(Environment.NewLine, LoremIpsum, LoremIpsum, LoremIpsum);
            const string text = LoremIpsum;
            var tags = new[] {"lorem", "ipsum", "test"};

            return Enumerable.Range(0, count).Select(i =>
                                                     new TestModelData
                                                         {
                                                             DataId = i.ToString("00000000"),
                                                             UserId = "tester",
                                                             Modified = new DateTime(2000, 1, 1).AddDays(i),
                                                             Title = "post " + i,
                                                             Text = text,
                                                             Tags = tags
                                                         }
                );
        }

        private static int BatchInsert(ICollection coll, int n)
        {
            return coll.ExecuteWrite(c =>
                                         {
                                             var count = 0;
                                             foreach (var post in GetTestData(n))
                                             {
                                                 ++count;
                                                 c.Update(post.DataId,
                                                          JSON.FromObject(post));
                                             }
                                             return count;
                                         });
        }
    }
}