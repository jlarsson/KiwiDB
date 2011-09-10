using System;
using System.Runtime.Serialization;

namespace KiwiDb
{
    public class KiwiDbException: ApplicationException
    {
        public KiwiDbException()
        {
        }

        public KiwiDbException(string message) : base(message)
        {
        }

        public KiwiDbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected KiwiDbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}