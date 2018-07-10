using System;
using System.Runtime.Serialization;

namespace CommaSeparatedValuesSerializer
{
    class NoDataException : Exception
    {
        public NoDataException()
        {
        }

        public NoDataException(string message) : base(message)
        {
        }

        public NoDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
