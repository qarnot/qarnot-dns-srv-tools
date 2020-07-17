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
    /// DnsServiceTargetSelectorReal class
    /// </summary>
    public class DnsServiceTargetSelectorReal : IDnsServiceTargetSelector
    {
        private IDnsSrvQuerier DnsQuerier { get; }

        private IDnsSrvSortResult DnsSortResult { get; }

        private DnsSrvQueryResult QueryResult { get; set; }

        private uint ServerRecoveryUnavailableTime { get; }

        private Semaphore SemaphoreKey;

        private ILogger Logger;

        private DnsSrvServiceDescription LastService;

        /// <summary>
        /// DnsServiceTargetSelectorReal constructor.
        /// </summary>
        /// <param name="dnsQuerier"></param>
        /// <param name="dnsSortResult"></param>
        /// <param name="serverRecoveryUnavailableTime"></param>
        /// <param name="logger"></param>
        public DnsServiceTargetSelectorReal(IDnsSrvQuerier dnsQuerier, IDnsSrvSortResult dnsSortResult, uint serverRecoveryUnavailableTime, ILogger logger = null)
        {
            DnsQuerier = dnsQuerier;
            DnsSortResult = dnsSortResult;
            ServerRecoveryUnavailableTime = serverRecoveryUnavailableTime;
            SemaphoreKey = new Semaphore(1, 1);
            Logger = logger;
        }

        private bool ShouldRetrieveResult => QueryResult == null || !QueryResult.IsAvailable;

        private async Task RetrieveQueryResultFromDnsAsync(DnsSrvServiceDescription service)
        {
            if (ShouldRetrieveResult)
            {
                Logger?.LogTrace($"Call the Dns Srv server");
                QueryResult = await DnsQuerier.QueryServiceAsync(service);
                DnsSortResult.Sort(QueryResult);
                LastService = service;
            }
        }

        internal void CheckService(DnsSrvServiceDescription service)
        {
            if (service == null)
            {
                Logger?.LogDebug($"DnsSrvServiceDescription service null found");
                throw new ArgumentNullException("service cannot be null");
            }
        }

        /// <summary>
        /// SelectHostAsync.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public async Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service)
        {
            SemaphoreKey.WaitOne();
            try
            {
                CheckService(service);
                if (!service.Equals(LastService) || ShouldRetrieveResult)
                {
                    await RetrieveQueryResultFromDnsAsync(service);
                }

                DnsSrvResultEntry entryFound = QueryResult?.DnsEntries?.FirstOrDefault(entry => entry.IsAvailable);

                if (entryFound != null)
                {
                    Logger?.LogTrace($"entry found {entryFound}");
                    return entryFound.DnsEndPoint;
                }

                QueryResult?.ReduceLiveTime(ServerRecoveryUnavailableTime);
                Logger?.LogTrace($"No entry found : 0 / {QueryResult?.DnsEntries?.Count ?? 0}");
                Logger?.LogTrace($"The DNS server will be recall at: {QueryResult?.TtlEndTime}");
            }
            finally
            {
                SemaphoreKey.Release();
            }
            return null;
        }

        /// <summary>
        /// BlacklistHostFor.
        /// </summary>
        /// <param name="dnsHost"></param>
        /// <param name="duration"></param>
        public void BlacklistHostFor(DnsEndPoint dnsHost, TimeSpan duration)
        {
            if (dnsHost == null)
            {
                Logger?.LogDebug($"Empty dnsHost given");
                throw new ArgumentNullException(nameof(dnsHost));
            }

            SemaphoreKey.WaitOne();
            try
            {
                QueryResult?.DnsEntries.ForEach(entry =>
                {
                    if (entry.HostName == dnsHost.Host && entry.Port == dnsHost.Port)
                    {
                        Logger?.LogTrace($"entry {entry} put in quarantine for : {duration}");
                        entry.PutInQuarantine(duration);
                    }
                });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        public void ResetBlacklistForHost(DnsEndPoint host)
        {
            SemaphoreKey.WaitOne();
            try
            {
                QueryResult?.DnsEntries?.ForEach(entry =>
                {
                    if (entry.HostName == host.Host && entry.Port == host.Port)
                    {
                        Logger?.LogTrace($"entry {entry} quarantine reset");
                        entry.ResetQuarantine();
                    }
                });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        public void Reset()
        {
            SemaphoreKey.WaitOne();
            try
            {
                Logger?.LogTrace($"Reset of DnsServiceTargetSelectorReal");
                QueryResult = null;
                LastService = null;
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }
    }
}