using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Transcribe
{
    public class NoFindContextException : ApplicationException
    {
        public NoFindContextException()
        {
        }

        public NoFindContextException(string message) : base(message)
        {
        }

        public NoFindContextException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoFindContextException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
