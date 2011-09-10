using System.Linq;
using KiwiDb.Gist.Tree;
using NUnit.Framework;

namespace KiwiDb.Tests.Gist.UpdateStrategies
{
    [TestFixture]
    public class AppendKeyFixture : GistFixtureBase
    {
        [Test]
        public void AppendKey()
        {
            var updateStrategy = new AppendKey<string, string>();

            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);
                gist.Insert("A", "value for A");
                blocks.AutoCommit = true;
            }

            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);
                gist.Insert("A", "second value for A");
                blocks.AutoCommit = true;
            }

            using (var blocks = CreateBlocks(false))
            {
                var gist = CreateGist(blocks, updateStrategy);
                CollectionAssert.AreEqual(new[] {"A", "A"}, gist.Scan().Select(kv => kv.Key).ToArray());
                CollectionAssert.AreEqual(new[] {"second value for A", "value for A"},
                                          gist.Scan().Select(kv => kv.Value).OrderBy(v => v).ToArray());
            }
        }

        [Test]
        public void BatchAppendKey()
        {
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, new AppendKey<string, string>());

                gist.Insert("A", "value for A");
                gist.Insert("A", "second value for A");
                CollectionAssert.AreEqual(new[] {"A", "A"}, gist.Scan().Select(kv => kv.Key).ToArray());
                CollectionAssert.AreEqual(new[] {"second value for A", "value for A"},
                                          gist.Scan().Select(kv => kv.Value).OrderBy(v => v).ToArray());

                blocks.AutoCommit = true;
            }
        }
    }
}