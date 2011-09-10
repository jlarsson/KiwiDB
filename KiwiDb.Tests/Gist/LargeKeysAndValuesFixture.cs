using System.Linq;
using NUnit.Framework;

namespace KiwiDb.Tests.Gist
{
    [TestFixture]
    public class LargeKeysAndValuesFixture : GistFixtureBase
    {
        [Test]
        public void LargeKeys()
        {
            // Generate records with very large keys (8k+)
            var data = Enumerable.Range(0, 100).
                ToDictionary(
                    i => i.ToString() + new string('.', 8*1024),
                    i => i.ToString());

            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks);

                foreach (var kv in data)
                {
                    gist.Insert(kv.Key, kv.Value);
                }
                blocks.AutoCommit = true;
            }
            using (var blocks = CreateBlocks(false))
            {
                var gist = CreateGist(blocks);

                foreach (var kv in data)
                {
                    var values = gist.Find(kv.Key).ToArray();

                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual(kv.Value, values[0].Value);
                }
            }
        }

        [Test]
        public void LargeValues()
        {
            // Generate records with very large values (16k+)
            var data = Enumerable.Range(0, 1000).
                ToDictionary(
                    i => i.ToString(),
                    i => i.ToString() + new string('.', 16*1024));

            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks);

                foreach (var kv in data)
                {
                    gist.Insert(kv.Key, kv.Value);
                }
                blocks.AutoCommit = true;
            }
            using (var blocks = CreateBlocks(false))
            {
                var gist = CreateGist(blocks);

                foreach (var kv in data)
                {
                    var values = gist.Find(kv.Key).ToArray();

                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual(kv.Value, values[0].Value);
                }
            }
        }
    }
}