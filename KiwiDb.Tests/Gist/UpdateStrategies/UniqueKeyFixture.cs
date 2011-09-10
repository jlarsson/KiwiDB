using System;
using KiwiDb.Gist.Tree;
using NUnit.Framework;

namespace KiwiDb.Tests.Gist.UpdateStrategies
{
    [TestFixture]
    public class UniqueKeyFixture : GistFixtureBase
    {
        [Test]
        public void BatchInsertingExistingKeyThrows()
        {
            var updateStrategy = new UniqueKey<string, string>();
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);

                gist.Insert("A", "value for A");

                Assert.Throws<ArgumentException>(() => gist.Insert("A", "new value for A"));
            }
        }

        [Test]
        public void InsertingExistingKeyThrows()
        {
            var updateStrategy = new UniqueKey<string, string>();
            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);

                gist.Insert("A", "value for A");
                blocks.AutoCommit = true;
            }

            using (var blocks = CreateBlocks(true))
            {
                var gist = CreateGist(blocks, updateStrategy);

                Assert.Throws<ArgumentException>(() => gist.Insert("A", "new value for A"));
            }
        }
    }
}