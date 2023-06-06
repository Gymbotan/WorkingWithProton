using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WorkingWithProton
{
    public class Exceptions
    {
        public class RequestException : Exception
        {
            public RequestException()
            {
            }

            public RequestException(string? message) : base(message)
            {
            }

            public RequestException(string? message, Exception? innerException) : base(message, innerException)
            {
            }
        }
    }
}
