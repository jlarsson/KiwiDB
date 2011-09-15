using System;

namespace PerfTest
{
    public class TestModelData
    {
        public string DataId { get; set; }
        public string UserId { get; set; }
        public DateTime Modified { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string[] Tags { get; set; }
    }
}