using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transcribe
{
    public class NoApiTimesException : ApplicationException
    {
        public NoApiTimesException()
        {
        }

        public NoApiTimesException(string message) : base(message)
        {
        }

        public NoApiTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoApiTimesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
