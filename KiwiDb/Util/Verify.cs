using System;

namespace KiwiDb.Util
{
    public static class Verify
    {
        public static void Argument(bool test, string format, params object[] args)
        {
            if (!test)
            {
                throw new ArgumentException(string.Format(format, args));
            }
        }

        public static Exception DuplicateKey(object duplicateKeyValue)
        {
            return new DuplicateKeyException(duplicateKeyValue);
        }
    }
}