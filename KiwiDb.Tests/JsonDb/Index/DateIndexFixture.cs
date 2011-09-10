using System;
using System.Linq;
using KiwiDb.JsonDb;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb.Index
{
    [TestFixture, Description("Verify that indices over dates behaves")]
    public class DateIndexFixture : IsolatedDatabaseFixture
    {
        public class Data
        {
            public DateTime Date { get; set; }
        }

        [Test]
        public void DefaultIndex()
        {
            var coll = GetCollection();

            // Create index over Date, considering all parts of DateTime values
            coll.EnsureIndex("Date");

            var date1 = DateTime.Now;
            var date2 = date1.AddSeconds(1);

            // Insert 2 dates
            coll.Update("1", new Data {Date = date1});
            coll.Update("2", new Data {Date = date2});

            // Look for first date only
            var foundDates = (from kv in coll.Find<Data>(new {Date = date1})
                              let date = kv.Value.Date
                              orderby date
                              select date).ToArray();

            CollectionAssert.AreEqual(
                new[] {date1},
                foundDates);
        }

        [Test]
        public void IgnoreTimeOfDay()
        {
            var coll = GetCollection();

            // Create index over Date, considering only the Date part and ignoring the time part
            coll.EnsureIndex("Date", new IndexOptions().SetIgnoreTimeOfDay(true));

            var date1 = new DateTime(2011, 09, 12);
            var date2 = date1.AddHours(2).AddMinutes(30);

            // Insert 2 dates
            coll.Update("1", new Data {Date = date1});
            coll.Update("2", new Data {Date = date2});

            // Look for first date only
            var foundDates = (from kv in coll.Find<Data>(new {Date = date1})
                              let date = kv.Value.Date
                              orderby date
                              select date).ToArray();

            CollectionAssert.AreEqual(
                new[] {date1, date2},
                foundDates);
        }

        [Test]
        public void Unique()
        {
            var coll = GetCollection();

            // Create unique index over Name
            coll.EnsureIndex("Date", new IndexOptions()
                                         .SetUnique(true)
                );

            var date = DateTime.Now;

            // Insert the date
            coll.Update("1", new Data {Date = date});

            // Insert the date again
            Assert.That(
                () => coll.Update("2", new Data {Date = date}),
                Throws.TypeOf<DuplicateKeyException>()
                );
        }

        [Test]
        public void UniqueIgnoreTimeOfDay()
        {
            var coll = GetCollection();

            // Create unique, time-of-day ignoring index over Name
            coll.EnsureIndex("Date", new IndexOptions()
                                         .SetUnique(true)
                                         .SetIgnoreTimeOfDay(true)
                );

            // 2 dates which differ in TimeSpan of day only
            var date1 = new DateTime(2011, 09, 12);
            var date2 = date1.AddHours(2).AddMinutes(30);

            coll.Update("1", new Data {Date = date1});

            Assert.That(
                () => coll.Update("2", new Data {Date = date2}),
                Throws.TypeOf<DuplicateKeyException>()
                );
        }
    }
}