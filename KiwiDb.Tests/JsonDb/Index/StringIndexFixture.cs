using System.Linq;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture, Description("Verify that indices over strings behaves")]
    public class StringIndexFixture : IsolatedDatabaseFixture
    {
        public class Data
        {
            public string Text { get; set; }
        }

        [Test]
        public void DefaultIndex()
        {
            var coll = GetCollection();

            // Create index over string
            coll.Indices.EnsureIndex("Text");

            // 2 values, different in case only, but considered totally different by the index
            const string text1 = "some text";
            const string text2 = "SOME TEXT";

            // Insert 2 dates
            coll.Update("1", new Data {Text = text1});
            coll.Update("2", new Data {Text = text2});

            // Look for first text only
            var foundTexts = (from kv in coll.Find<Data>(new {Text = text1})
                              let text = kv.Value.Text
                              orderby text
                              select text).ToArray();

            CollectionAssert.AreEqual(
                new[] {text1},
                foundTexts);
        }

        [Test]
        public void IgnoreCase()
        {
            var coll = GetCollection();

            // Create a case insensitive index over Text
            coll.Indices.EnsureIndex("Text", new IndexOptions() { WhenStringThenIgnoreCase = true });

            // some values, different in case only, but considered equal by the index
            const string text1 = "some text";
            const string text2 = "SOME TEXT";
            const string searchText = "SoMe TeXt";

            // Insert 2 dates
            coll.Update("1", new Data {Text = text1});
            coll.Update("2", new Data {Text = text2});

            // Look for first date only
            var foundDates = (from kv in coll.Find<Data>(new {Text = searchText})
                              let text = kv.Value.Text
                              orderby text
                              select text).ToArray();

            CollectionAssert.AreEqual(
                new[] {text1, text2},
                foundDates);
        }

        [Test]
        public void Unique()
        {
            var coll = GetCollection();

            // Create unique, case insensitive index over Name
            coll.Indices.EnsureIndex("Text", new IndexOptions(){IsUnique = true});

            // Insert
            coll.Update("1", new Data {Text = "some text"});

            // Insert the same value again
            Assert.That(
                () => coll.Update("2", new Data {Text = "some text"}),
                Throws.TypeOf<DuplicateKeyException>()
                );
        }

        [Test]
        public void UniqueIgnoreCase()
        {
            var coll = GetCollection();

            // Create unique, case insensitive index over Name
            coll.Indices.EnsureIndex("Text", new IndexOptions(){IsUnique = true, WhenStringThenIgnoreCase = true});

            // Insert
            coll.Update("1", new Data {Text = "some text"});

            // Insert a similar value again
            Assert.That(
                () => coll.Update("2", new Data {Text = "SOME TEXT"}),
                Throws.TypeOf<DuplicateKeyException>()
                );
        }
    }
}