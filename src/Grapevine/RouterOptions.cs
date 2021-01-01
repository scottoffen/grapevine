namespace Grapevine
{
    public abstract class RouterOptions
    {
        /// <summary>
        /// Gets or sets a value to indicate whether request routing should continue even after a response has been sent.
        /// </summary>
        /// <value>false</value>
        public bool ContinueRoutingAfterResponseSent { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether autoscan is enabled on this router.
        /// </summary>
        /// <value>true</value>
        public bool EnableAutoScan { get; set; } = true;

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
    }
}