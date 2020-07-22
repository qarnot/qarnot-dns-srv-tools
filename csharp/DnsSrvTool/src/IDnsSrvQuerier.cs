namespace DnsSrvTool
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IDnsSrvQuerier interface.
    /// </summary>
    public interface IDnsSrvQuerier
    {
        /// <summary>
        /// Ask a new srv call.
        /// </summary>
        /// <param name="service">Address to be call.</param>
        /// <returns>Srv Call Response.</returns>
        Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service);
    }
}