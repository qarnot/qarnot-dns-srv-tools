namespace QarnotDsnHandler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Get specific uri interface
    /// </summary>
    public interface IGetUri
    {
        /// <summary>
        /// Get uri model
        /// </summary>
        /// <returns>Backend server uri.</returns>
        Task<Uri> BalanceApiServerUri(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Is a Qarnot uri
        /// </summary>
        bool DnsSrvFind { get; }


        /// <summary>
        /// Put the backend in quarantine and choose another backend.
        /// </summary>
        /// <returns>Backend server uri.</returns>
        Task<Uri> NextApiUri(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the Uri from the current given address
        /// </summary>
        Uri GetUri(string call = null);
    }
}