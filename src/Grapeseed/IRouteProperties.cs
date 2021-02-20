namespace Grapevine
{
    public interface IRouteProperties
    {
        /// <summary>
        /// Gets or sets an optional description for the route
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the route is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the HttpMethod that this route responds to; defaults to HttpMethod.Any
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets a unique name for function that will be invoked in the route, internally assigned
        /// </summary>
        string Name { get; }
    }
}