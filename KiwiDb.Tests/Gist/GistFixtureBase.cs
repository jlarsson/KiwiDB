using System;
using System.IO;
using KiwiDb.Gist.Extensions;
using KiwiDb.Gist.Tree;
using KiwiDb.Storage;
using NUnit.Framework;

namespace KiwiDb.Tests.Gist
{
    public class GistFixtureBase
    {
        private readonly string _databasePath = Path.GetFullPath(@".\test.kiwidb");

        public GistFixtureBase()
        {
            _databasePath = Path.GetFullPath(@".\" + GetType().Name + ".kiwidb");
        }

        [SetUp]
        public void SetUp()
        {
            File.Delete(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
        }

        protected virtual IBlockCollection CreateBlocks(bool allowWrite)
        {
            Console.Out.WriteLine("## Database file: " + _databasePath);
            return allowWrite
                       ? FileStreamBlockCollection.CreateWrite(_databasePath)
                       : FileStreamBlockCollection.CreateRead(_databasePath);
        }

        protected virtual Gist<string, string> CreateGist(IBlockCollection blocks)
        {
            return CreateGist(blocks, new UpdateKey<string, string>());
        }

        protected Gist<string, string> CreateGist(IBlockCollection blocks,
                                                  IUpdateStrategy<string, string> updateStrategy)
        {
            return new Gist<string, string>(
                blocks.MasterBlockReference,
                new GistConfig<string, string>
                    {
                        Blocks = blocks,
                        Ext = new OrderedGistExtension<string, string>(
                            new GistStringType(),
                            new GistStringType()
                            ),
                        UpdateStrategy = updateStrategy
                    }
                );
        }
    }
}