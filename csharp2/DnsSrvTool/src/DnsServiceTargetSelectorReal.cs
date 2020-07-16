namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1054, SA1611, CS1591
    // And a concrete implementation
    public class DnsServiceTargetSelectorReal : IDnsServiceTargetSelector
    {
        private IDnsSrvQuerier DnsQuerier { get; }

        private IDnsSrvSortResult DnsSortResult { get; }

        private DnsSrvQueryResult QueryResult { get; set; }

        private uint ResultCacheTimeTtl { get; set; }

        private uint ServerRecoveryUnavailableTime { get; set; }

        private DateTime ServerRecoveryPeriod { get; set; }

        private Semaphore SemaphoreKey;

        private bool WaitForSeverRecovery => ServerRecoveryPeriod > DateTime.UtcNow;

        public DnsServiceTargetSelectorReal(IDnsSrvQuerier dnsQuerier, IDnsSrvSortResult dnsSortResult, uint resultCacheTtlInSecond, uint resultRecoveryTtlFromFailInSecond)
        {
            DnsQuerier = dnsQuerier;
            DnsSortResult = dnsSortResult;
            ResultCacheTimeTtl = resultCacheTtlInSecond;
            ServerRecoveryUnavailableTime = resultRecoveryTtlFromFailInSecond; // extract to the class?
            ServerRecoveryPeriod = DateTime.UtcNow;
            SemaphoreKey = new Semaphore(1, 1);
        }

        private bool ShouldRetrieveResult => QueryResult == null || !QueryResult.IsAvailable;
        private async Task RetrieveQueryResultFromDnsAsync(DnsSrvServiceDescription service)
        {
            SemaphoreKey.WaitOne();
            try
            {
            if (ShouldRetrieveResult)
            {
                QueryResult = await DnsQuerier.QueryServiceAsync(service);
                QueryResult = DnsSortResult.Sort(QueryResult);
            }
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }


        // should verify the service
        public async Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }

            // // if fail increase the
            // // wait time before retrieve result
            // if (WaitForSeverRecovery)
            // {
            //     // return test quarantaine
            //     // if no quarantaine reset
            //     return null;
            // }

            if (ShouldRetrieveResult)
            {
                await RetrieveQueryResultFromDnsAsync(service);
            }


            DnsSrvResultEntry entryFound = null;
            SemaphoreKey.WaitOne();
            try
            {
                entryFound = QueryResult?.DnsEntries?.FirstOrDefault(entry => entry.IsAvailable);

                if (entryFound == null)
                {
                    // ????
                    // ServerRecoveryPeriod = DateTime.UtcNow.AddSeconds(ServerRecoveryUnavailableTime);
                    return null;
                }
            }
            finally
            {
                SemaphoreKey.Release();
            }

            return entryFound.DnsEndPoint;
        }

        // Blacklist a host for some time. No questions asked.
        public void BlacklistHostFor(DnsEndPoint dnsHost, TimeSpan duration)
        {
            if (dnsHost == null)
            {
                throw new ArgumentNullException(nameof(dnsHost));
            }


            SemaphoreKey.WaitOne();
            try
            {
            QueryResult?.DnsEntries.ForEach(entry =>
            {
                if (entry.HostName == dnsHost.Host && entry.Port == dnsHost.Port)
                {
                    entry.PutInQuarantine(duration);
                }
            });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        // Immediately remove a host from blacklist
        public void ResetBlacklistForHost(DnsEndPoint host)
        {

            SemaphoreKey.WaitOne();
            try
            {
            QueryResult?.DnsEntries?.ForEach(entry =>
            {
                if (entry.HostName == host.Host && entry.Port == host.Port)
                {
                    entry.ResetQuarantine();
                }
            });
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        // Most implementation (except mocks) will be stateful, a Reset() method will be handy.
        public void Reset()
        {

            SemaphoreKey.WaitOne();
            try
            {
            ServerRecoveryPeriod = DateTime.UtcNow;
            QueryResult = null;
            }
            finally
            {
                SemaphoreKey.Release();
            }
        }

        // In this class, you will implement logic to handle querying, retrying queries
        // to DNS if they fail, timing out the blacklisting, caching the query result for
        // some time, ...

        // You will NOT handle:
        //      - building URIs (here we don't know what an URI is
        //      - making decisions about what should be blacklisted
    }
}