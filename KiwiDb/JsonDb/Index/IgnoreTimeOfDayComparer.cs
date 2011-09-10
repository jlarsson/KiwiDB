using System;
using System.Collections.Generic;

namespace KiwiDb.JsonDb.Index
{
    public class IgnoreTimeOfDayComparer : IComparer<DateTime>
    {
        private readonly IComparer<DateTime> _defaultComparer = Comparer<DateTime>.Default;
        public int Compare(DateTime x, DateTime y)
        {
            return _defaultComparer.Compare(x.Date, y.Date);
        }
    }
}