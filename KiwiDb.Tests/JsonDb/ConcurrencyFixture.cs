using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace KiwiDb.Tests.JsonDb
{
    [TestFixture]
    public class ConcurrencyFixture: IsolatedDatabaseFixture
    {
        [Test, Explicit]
        public void ConcurrentReadIsUnrestricted()
        {
            var numberOfActiveReaders = 0;
            const int readerCount = 4;
            var allReadersActiveEvent = new ManualResetEvent(false);

            var tasks = (from i in Enumerable.Range(0, readerCount)
                        select new Task(() => GetCollection().ExecuteRead(c =>
                                                                              {
                                                                                  //Console.Out.WriteLine("starting thread " + i);
                                                                                  c.Find(null);
                                                                                  if (Interlocked.Increment(ref numberOfActiveReaders) == readerCount-1)
                                                                                  {
                                                                                      //Console.Out.WriteLine("thread " + i + " is the last");
                                                                                      allReadersActiveEvent.Set();
                                                                                  }

                                                                                  if (!allReadersActiveEvent.WaitOne(10000))
                                                                                  {
                                                                                      throw new Exception("apa");
                                                                                  }
                                                                                  //Console.Out.WriteLine("exiting thread " + i);
                                                                                  return 0;
                                                                              }))).ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }


            Task.WaitAll(tasks);
        }

        [Test]
        public void OnlyOneWriterIsAllowed()
        {
            var enteredConflictingWriter = false;
            var enteredWinningWriter = false;
            var conflictingWriter = new Task(() => GetCollection().ExecuteWrite(c =>
                                                                                    {
                                                                                        enteredConflictingWriter = true;
                                                                                        Assert.Fail("SHOULD NOT HAPPEN, WOULD LEAD TO CONCURRENT WRITES");
                                                                                        return false;
                                                                                    }));

            // The winning writer will block until the second writer is terminated
            var winningWriter = new Task(() => GetCollection().ExecuteWrite(c =>
                                                                                {
                                                                                    // Start write operation in another thread
                                                                                    conflictingWriter.Start();

                                                                                    // Which, of course should fail
                                                                                    Assert.Throws<AggregateException>(conflictingWriter.Wait);

                                                                                    enteredWinningWriter = true;
                                                                                    return true;
                                                                                }));


            winningWriter.Start();
            winningWriter.Wait();

            Assert.IsTrue(enteredWinningWriter);
            Assert.IsFalse(enteredConflictingWriter);
        }

        [Test]
        public void ReadersAndWriters()
        {
            var sync = new object();
            var writeOperations = 0;
            var readOperations = 0;
            var readerCount = 0;
            var writerCount = 0;
            var maxConcurrentReaders = 0;
            var maxConcurrentWriters = 0;

            var quitEvent = new ManualResetEvent(false);

            DatabaseFileProvider.Timeout = TimeSpan.FromSeconds(10);

            var writers = from i in Enumerable.Range(0, 5) select new Task(() =>
                                      {
                                          while (!quitEvent.WaitOne(0))
                                          {
                                              GetCollection().ExecuteWrite(c =>
                                                                              {

                                                                                  lock (sync)
                                                                                  {
                                                                                      ++writeOperations;
                                                                                      ++writerCount;
                                                                                      maxConcurrentWriters =
                                                                                          Math.Max(
                                                                                              maxConcurrentWriters,
                                                                                              writerCount);
                                                                                  }
                                                                                  Thread.Sleep(100);
                                                                                  lock (sync)
                                                                                  {
                                                                                      --writerCount;
                                                                                  }
                                                                                  return 0;
                                                                              });
                                          }

                                      }
                );
            var readers = from i in Enumerable.Range(0, 5)
                          select new Task(() =>
                                              {
                                                  while (!quitEvent.WaitOne(0))
                                                  {
                                                      GetCollection().ExecuteRead(c =>
                                                                                      {

                                                                                          lock (sync)
                                                                                          {
                                                                                              ++readOperations;
                                                                                              ++readerCount;
                                                                                              maxConcurrentReaders = Math.Max( maxConcurrentReaders,readerCount);
                                                                                          }
                                                                                          Thread.Sleep(100);
                                                                                          lock (sync)
                                                                                          {
                                                                                              --readerCount;
                                                                                          }
                                                                                          return 0;
                                                                                      });
                                                  }
                                              }
                              );

            var tasks = new List<Task>(readers.Concat(writers)).ToArray();
            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks, TimeSpan.FromSeconds(5));

            quitEvent.Set();
            Task.WaitAll(tasks);

            Console.Out.WriteLine("Read/write operations: {0}/{1}", readOperations, writeOperations);
            Console.Out.WriteLine("Read/write concurrency max: {0}/{1}", maxConcurrentReaders, maxConcurrentWriters);
        }
    }
}