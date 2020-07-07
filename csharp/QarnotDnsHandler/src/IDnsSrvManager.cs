namespace QarnotDnsHandler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1054, SA1611

    /// <summary>
    /// Get specific uri interface.
    /// </summary>
    public interface IDnsSrvManager
    {
        /// <summary>
        /// Get the Uri from the current given address.
        /// </summary>
        /// <param name="uriValue">the path uri to call.</param>
        /// <returns>The api uri get.</returns>
        Uri GetUri(string uriValue = null);

        /// <summary>
        /// Get uri model.
        /// </summary>
        /// <returns>Backend server uri.</returns>
        Task<Uri> BalanceApiServerUri(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Put the backend in quarantine and choose another backend.
        /// </summary>
        /// <returns>Backend server uri.</returns>
        bool NextApiUri();
    }
}