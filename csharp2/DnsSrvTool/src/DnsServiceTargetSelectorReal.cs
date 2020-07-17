namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class DnsServiceTargetSelectorReal : IDnsServiceTargetSelector
    {
        private IDnsSrvQuerier DnsQuerier { get; }

        private IDnsSrvSortResult DnsSortResult { get; }

        private DnsSrvQueryResult QueryResult { get; set; }

        private uint ResultCacheTimeTtl { get; }

        private uint ServerRecoveryUnavailableTime { get; }

        private DateTime ServerRecoveryPeriod { get; }

        private Semaphore SemaphoreKey;

        private ILogger Logger;

        private bool WaitForSeverRecovery => ServerRecoveryPeriod > DateTime.UtcNow;

        public DnsServiceTargetSelectorReal(IDnsSrvQuerier dnsQuerier, IDnsSrvSortResult dnsSortResult, uint resultCacheTtlInSecond, uint resultRecoveryTtlFromFailInSecond, ILogger logger = null)
        {
            DnsQuerier = dnsQuerier;
            DnsSortResult = dnsSortResult;
            ResultCacheTimeTtl = resultCacheTtlInSecond;
            ServerRecoveryUnavailableTime = resultRecoveryTtlFromFailInSecond; // extract to the class?
            ServerRecoveryPeriod = DateTime.UtcNow;
            SemaphoreKey = new Semaphore(1, 1);
            Logger = logger;
        }

        private bool ShouldRetrieveResult => QueryResult == null || !QueryResult.IsAvailable;

        private async Task RetrieveQueryResultFromDnsAsync(DnsSrvServiceDescription service)
        {
            SemaphoreKey.WaitOne();
            try
            {
                if (ShouldRetrieveResult)
                {
                    Logger?.LogInformation($"Retrieve Dns call");

                    QueryResult = await DnsQuerier.QueryServiceAsync(service);
                    Logger?.LogTrace($"Dns result create ");
                    DnsSortResult.Sort(QueryResult);
                }
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        public async Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }

            if (ShouldRetrieveResult)
            {
                await RetrieveQueryResultFromDnsAsync(service);
            }

            SemaphoreKey.WaitOne();
            try
            {
                DnsSrvResultEntry entryFound = QueryResult?.DnsEntries?.FirstOrDefault(entry => entry.IsAvailable);

                if (entryFound != null)
                {
                    Logger?.LogTrace($"entry found {entryFound}");
                    return entryFound.DnsEndPoint;
                }

                QueryResult?.ReduceLiveTime(ServerRecoveryUnavailableTime);
                Logger?.LogTrace($"No entry found ");
                Logger?.LogTrace($"restore DNS entries in {QueryResult?.TtlEndTime}");
            }
            finally
            {
                SemaphoreKey.Release();
            }
            return null;

        }

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
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }
    }
}