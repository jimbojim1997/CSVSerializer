using System;
using System.Runtime.Serialization;

namespace CommaSeparatedValuesSerializer
{
    class InvalidTypeException : Exception
    {
        public InvalidTypeException()
        {
        }

        public InvalidTypeException(string message) : base(message)
        {
        }

        public InvalidTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
