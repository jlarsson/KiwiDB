using System;
using System.IO;
using KiwiDb;

namespace PerfTest
{
    public class KiwiPerformanceTest
    {
        public KiwiPerformanceTest()
        {
            DatabasePath = Path.GetFullPath(".\\test-" + Guid.NewGuid().ToString("n") + ".kiwidb");
        }

        public string DatabasePath { get; private set; }
        public string Description { get; set; }

        public Func<ICollection, int, int> Test { get; set; }

        public void Run(int n, ITestLog log)
        {
            var path = Path.GetFullPath(".\\test-" + Guid.NewGuid().ToString("n") + ".kiwidb");
            try
            {
                log.Start();
                n = Test(new Collection(path), n);
                log.Stop(n);
            }
            finally
            {
                File.Delete(path);
                
            }
        }
    }
}