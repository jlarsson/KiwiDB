using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Kiwi.Json;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.PerformanceTests
{
    [TestFixture, Explicit]
    public class InsertPostsFixture
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetFullPath(@".\" + GetType().Name + ".kiwidb");

            File.Delete(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        private const string LoremIpsum =
            @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec feugiat pharetra dignissim. Duis feugiat dui ut tellus aliquam consectetur. Ut a pulvinar lorem. Pellentesque at elit in neque faucibus condimentum ac nec justo. Integer tincidunt eleifend justo. Nulla sed metus a est tempor aliquam et in lacus. Donec imperdiet cursus elit vitae sodales. Suspendisse lobortis condimentum lorem vitae volutpat. Donec scelerisque condimentum vestibulum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam sollicitudin erat vel tortor consequat eu convallis neque posuere. Aenean tortor eros, ultrices a vulputate sed, mollis at neque. Donec mollis scelerisque mi, at porta dui adipiscing et.";

        private string _databasePath;

        private Collection GetCollection()
        {
            return new Collection(_databasePath);
        }

        public class Post
        {
            public string PostId { get; set; }
            public string UserId { get; set; }
            public DateTime Modified { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public string[] Tags { get; set; }
        }

        public IEnumerable<Post> GetPosts(int count)
        {
            //var text = string.Join(Environment.NewLine, LoremIpsum, LoremIpsum, LoremIpsum);
            var text = LoremIpsum;
            var tags = new[] {"lorem", "ipsum", "test"};

            return Enumerable.Range(0, count).Select(i =>
                                                     new Post
                                                         {
                                                             PostId = i.ToString("00000000"),
                                                             UserId = "tester",
                                                             Modified = new DateTime(2000, 1, 1).AddDays(i),
                                                             Title = "post " + i,
                                                             Text = text,
                                                             Tags = tags
                                                         }
                );
        }

        private void VerifyPosts(int n)
        {
            var posts = GetPosts(n).ToDictionary(p => p.PostId, p => p);
            Run("Verifying", coll =>
                                 {
                                     foreach (var post in posts.Values)
                                     {
                                         Assert.AreEqual(coll.Get(post.PostId).ToString(),
                                                         JSON.FromObject(post).ToString());
                                     }
                                     return posts.Count;
                                 });
        }

        private void Run(string comment, Func<ICollection, int> action)
        {
            var coll = GetCollection();

            Console.Out.WriteLine(comment);

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var n = action(coll);
            stopWatch.Stop();
            Console.Out.WriteLine("{0} operations took {1} - {2} ops/s", n, stopWatch.Elapsed,
                                  n/stopWatch.Elapsed.TotalMilliseconds*1000);
        }

        [Test]
        public void BatchUpdate()
        {
            const int n = 5000;
            Run("Updating posts in same batch", c => c.ExecuteWrite(coll =>
                                                                        {
                                                                            var count = 0;
                                                                            foreach (var post in GetPosts(n))
                                                                            {
                                                                                ++count;
                                                                                coll.Update(post.PostId,
                                                                                            JSON.FromObject(post));
                                                                            }
                                                                            return count;
                                                                        })
                );
            VerifyPosts(n);
        }

        [Test]
        public void BatchUpdateWithIndex()
        {
            const int n = 5000;
            GetCollection().EnsureIndex("UserId");
            Run("Updating posts in same batch", c => c.ExecuteWrite(coll =>
                                                                        {
                                                                            var count = 0;
                                                                            foreach (var post in GetPosts(n))
                                                                            {
                                                                                ++count;
                                                                                coll.Update(post.PostId,
                                                                                            JSON.FromObject(post));
                                                                            }
                                                                            return count;
                                                                        })
                );
            VerifyPosts(n);
        }

        [Test]
        public void Update()
        {
            const int n = 5000;
            Run("Updating posts individally", c =>
                                                  {
                                                      var count = 0;
                                                      foreach (var post in GetPosts(n))
                                                      {
                                                          ++count;
                                                          c.Update(post.PostId, JSON.FromObject(post));
                                                      }
                                                      return count;
                                                  });

            VerifyPosts(n);
        }

        [Test]
        public void UpdateWithIndex()
        {
            const int n = 5000;
            GetCollection().EnsureIndex("UserId");
            Run("Updating posts individally", c =>
                                                  {
                                                      var count = 0;
                                                      foreach (var post in GetPosts(n))
                                                      {
                                                          ++count;
                                                          c.Update(post.PostId, JSON.FromObject(post));
                                                      }
                                                      return count;
                                                  });

            VerifyPosts(n);
        }
    }
}