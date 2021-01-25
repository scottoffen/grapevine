using System;

namespace Grapevine
{
    public class StatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public StatusCodeException(HttpStatusCode statusCode) : base()
        {
            StatusCode = statusCode;
        }
    }
}