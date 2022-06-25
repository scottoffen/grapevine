using System;

namespace Grapeseed.Exceptions
{
    public class NoOpenPortException : Exception
    {
        public NoOpenPortException() : base()
        {

        }
        public NoOpenPortException(string message) : base(message)
        {

        }
    }
}
