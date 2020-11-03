#pragma warning disable CA1303, CA1307

namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// DnsServiceTargetSelectorReal class.
    /// </summary>
    public class DnsServiceTargetSelectorReal : IDnsServiceTargetSelector, IDisposable
    {
        private SemaphoreSlim SemaphoreKey;

        private ILogger Logger;

        private DnsSrvServiceDescription LastService;

        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServiceTargetSelectorReal"/> class.
        /// </summary>
        /// <param name="dnsQuerier">caller of the dns.</param>
        /// <param name="dnsSortResult">Sort the results.</param>
        /// <param name="serverRecoveryUnavailableTime">recovery max-time if the server addresses are down, (should be lower than the time to live given by the dns).</param>
        /// <param name="logger">Optional Logger if logs are necessary.</param>
        public DnsServiceTargetSelectorReal(IDnsSrvQuerier dnsQuerier, IDnsSrvSortResult dnsSortResult, uint serverRecoveryUnavailableTime, ILogger logger = null)
        {
            DnsQuerier = dnsQuerier;
            DnsSortResult = dnsSortResult;
            ServerRecoveryUnavailableTime = serverRecoveryUnavailableTime;
            SemaphoreKey = new SemaphoreSlim(1, 1);
            Logger = logger;
        }

        private IDnsSrvQuerier DnsQuerier { get; }

        private IDnsSrvSortResult DnsSortResult { get; }

        private DnsSrvQueryResult QueryResult { get; set; }

        private uint ServerRecoveryUnavailableTime { get; }

        private bool ShouldRetrieveResult => QueryResult == null || !QueryResult.IsAvailable;

        /// <summary>
        /// Retrive the chosen DNS response endPoint.
        /// </summary>
        /// <param name="service">Dns service to call.</param>
        /// <exception cref="ArgumentNullException">Thrown when service is null.</exception>
        /// <exception cref="Exception">Thrown when service is different to the last service call.</exception>
        /// <returns>the DnsEndpoint response or null if no endPoint found.</returns>
        public async Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service)
        {
            await SemaphoreKey.WaitAsync();
            try
            {
                CheckService(service);

                if (ShouldRetrieveResult)
                {
                    await RetrieveQueryResultFromDnsAsync(service);
                }

                DnsSrvResultEntry entryFound = QueryResult?.DnsEntries?.FirstOrDefault(entry => entry.IsAvailable);

                if (entryFound != null)
                {
                    Logger?.LogDebug("Entry found {entryFound}", entryFound);
                    return entryFound.DnsEndPoint;
                }

                QueryResult?.ReduceLifeTime(ServerRecoveryUnavailableTime);
                Logger?.LogDebug("No entry found : 0 / {entryCount}", QueryResult?.DnsEntries?.Count ?? 0);
                Logger?.LogDebug("The DNS server will be recall at: {QueryResultTtlEndTime}", QueryResult?.TtlEndTime);
            }
            finally
            {
                SemaphoreKey.Release();
            }

            return null;
        }

        /// <summary>
        /// Blacklist a DNS response endpoint.
        /// </summary>
        /// <param name="dnsHost">Host to be blacklist.</param>
        /// <param name="duration">Blacklist duration.</param>
        /// <returns>The Task return.</returns>
        public async Task BlacklistHostForAsync(DnsEndPoint dnsHost, TimeSpan duration)
        {
            if (dnsHost == null)
            {
                Logger?.LogError("Empty dnsHost given to the BlacklistHostFor");
                throw new ArgumentNullException(nameof(dnsHost));
            }

            await SemaphoreKey.WaitAsync();
            try
            {
                QueryResult?.DnsEntries.ForEach(entry =>
                {
                    if (entry.HostName == dnsHost.Host && entry.Port == dnsHost.Port)
                    {
                        Logger?.LogDebug("entry {entry} put in Quarantine for : {duration}", entry, duration);
                        entry.PutInQuarantine(duration);
                    }
                });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        /// <summary>
        /// Reset a blacklisted host.
        /// </summary>
        /// <param name="host">Host to be reset.</param>
        /// <returns>The Task return.</returns>
        public async Task ResetBlacklistForHostAsync(DnsEndPoint host)
        {
            await SemaphoreKey.WaitAsync();
            try
            {
                QueryResult?.DnsEntries?.ForEach(entry =>
                {
                    if (entry.HostName == host.Host && entry.Port == host.Port)
                    {
                        Logger?.LogDebug("entry {entry} Quarantine reset", entry);
                        entry.ResetQuarantine();
                    }
                });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        /// <summary>
        /// Reset all the DnsSrvServiceDescription values.
        /// </summary>
        /// <returns>The Task return.</returns>
        public async Task ResetAsync()
        {
            await SemaphoreKey.WaitAsync();
            try
            {
                Logger?.LogDebug("Reset of DnsServiceTargetSelectorReal");
                QueryResult = null;
                LastService = null;
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        /// <summary>
        /// Dispose the DnsServiceTargetSelectorReal.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void CheckService(DnsSrvServiceDescription service)
        {
            if (service == null)
            {
                Logger?.LogError("DnsSrvServiceDescription service null found");
                throw new ArgumentNullException(nameof(service));
            }

            if (LastService != null && !service.Equals(LastService))
            {
                Logger?.LogError("Cannot change the resolved service");
                throw new Exception("Cannot change the resolved service");
            }
        }

        /// <summary>
        /// Dispose the DnsServiceTargetSelectorReal.
        /// </summary>
        /// <param name="disposing">Dispose the sub objects.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    SemaphoreKey.Dispose();
                }

                disposed = true;
            }
        }

        private async Task RetrieveQueryResultFromDnsAsync(DnsSrvServiceDescription service)
        {
            Logger?.LogDebug("Retrieve DnsService values {service}", service);
            QueryResult = await DnsQuerier.QueryServiceAsync(service);
            DnsSortResult.Sort(QueryResult);
            LastService = service;
        }
    }
}