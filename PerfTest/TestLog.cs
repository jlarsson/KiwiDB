using System;
using System.Diagnostics;

namespace PerfTest
{
    public class TestLog : ITestLog
    {
        private readonly KiwiPerformanceTest _test;
        readonly Stopwatch _stopwatch = new Stopwatch();

        public TestLog(KiwiPerformanceTest test)
        {
            _test = test;
        }

        public void Start()
        {
            Console.Out.WriteLine("/testrunner: " + _test.Description);
            _stopwatch.Start();
        }

        public void Stop(int n)
        {
            _stopwatch.Stop();
            Console.Out.WriteLine("{0} operations took {1} - {2} ops/s", n, _stopwatch.Elapsed,
                                  n / _stopwatch.Elapsed.TotalMilliseconds * 1000);

        }
    }
}