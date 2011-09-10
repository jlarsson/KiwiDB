using System.IO;
using System.Runtime.InteropServices;

namespace KiwiDb.Storage
{
    public static class IoError
    {
        public static bool IsSharingViolation(IOException e)
        {
            var hr = Marshal.GetHRForException(e);
            return (hr & 0xFFFF) == 32;
        }
    }
}