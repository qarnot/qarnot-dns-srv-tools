namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDnsServiceTargetSelector interface.
    /// </summary>
    public interface IDnsServiceTargetSelector
    {
        /// <summary>
        /// Retrive the chosen DNS response endPoint.
        /// </summary>
        /// <param name="service">Dns service to call.</param>
        /// <returns>the DnsEndpoint response or null if no endPoint found.</returns>
        Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service);

        /// <summary>
        /// Blacklist a response endpoint.
        /// </summary>
        /// <param name="host">Host to be blacklist.</param>
        /// <param name="duration">Blacklist duration.</param>
        /// <returns>The Task return.</returns>
        Task BlacklistHostForAsync(DnsEndPoint host, TimeSpan duration);

        /// <summary>
        /// Reset a blacklisted endpoint.
        /// </summary>
        /// <param name="host">Endpoint to be reset.</param>
        /// <returns>The Task return.</returns>
        Task ResetBlacklistForHostAsync(DnsEndPoint host);

        /// <summary>
        /// Reset all the object values.
        /// </summary>
        /// <returns>The Task return.</returns>
        Task ResetAsync();
    }
}