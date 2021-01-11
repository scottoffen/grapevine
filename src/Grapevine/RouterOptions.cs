using System;

namespace Grapevine
{
    public class RouterOptions
    {
        /// <summary>
        /// Gets or sets a value to indicate whether request routing should continue even after a response has been sent.
        /// </summary>
        /// <value>false</value>
        public bool ContinueRoutingAfterResponseSent { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate whether routing should be required to send a response.
        /// </summary>
        /// <value>true</value>
        public bool RequireRouteResponse { get; set; } = true;

        /// <summary>
        /// Gets or sets a value to indicate that before and after routing handlers should be executed even when no matching routes are found.
        /// </summary>
        /// <value>false</value>
        public bool RouteAnyway { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate whether exception text, where available, should be sent in http responses
        /// </summary>
        /// <value>false</value>
        public bool SendExceptionMessages { get; set; }

        /// <summary>
        /// Gets or sets a timespan value to use when setting the Expires header for static content
        /// </summary>
        /// <value>TimeSpan.FromDays(1)</value>
        public TimeSpan ContentExpiresDuration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Gets or sets an integer to indicate the minimum number of bytes after which content will potentially be compressed before being returned to the client
        /// </summary>
        /// <value>1024</value>
        public int CompressIfBytesGreaterThan { get; set; } = 1024;
    }
}