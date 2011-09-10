using System.Linq;
using KiwiDb.Gist.Tree;
using NUnit.Framework;

namespace KiwiDb.Tests.Gist.UpdateStrategies
{
    [TestFixture]
    public class UpdateExistingKeyFixture : GistFixtureBase
    {
        [Test]
        public void BatchUpdateExisting()
        {
            var updateStrategy = new UpdateKey<string, string>();
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);

                gist.Insert("A", "value for A");
                gist.Insert("A", "new value for A");

                CollectionAssert.AreEqual(new[] {"A"}, gist.Scan().Select(kv => kv.Key).ToArray());
                CollectionAssert.AreEqual(new[] {"new value for A"}, gist.Scan().Select(kv => kv.Value).ToArray());
            }
        }

        [Test]
        public void UpdateExisting()
        {
            var updateStrategy = new UpdateKey<string, string>();
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);
                gist.Insert("A", "value for A");
                blocks.AutoCommit = true;
            }
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);
                gist.Insert("A", "new value for A");
                blocks.AutoCommit = true;
            }

            using (var blocks = CreateBlocks(false))
            {
                var gist = CreateGist(blocks, updateStrategy);

                CollectionAssert.AreEqual(new[] {"A"}, gist.Scan().Select(kv => kv.Key).ToArray());
                CollectionAssert.AreEqual(new[] {"new value for A"}, gist.Scan().Select(kv => kv.Value).ToArray());
            }
        }
    }
}