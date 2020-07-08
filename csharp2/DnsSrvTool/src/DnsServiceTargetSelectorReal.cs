namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    // And a concrete implementation
    public class DnsServiceTargetSelectorReal : IDnsServiceTargetSelector
    {
        private IDnsSrvQuerier DnsQuerier { get; }

        private DnsSrvQueryResult QueryResult { get; set; }

        private int ResultCacheTimeTtl { get; set; }

        private int ResultFailTtl { get; set; }

        private int ServerRecoveryTIme { get; set; }
        private DateTime ServerRecoveryTtl { get; set; }

        private SemaphoreSlim Semaphore;

        private bool WaitForSeverRecovery()
        {
            return ServerRecoveryTtl > DateTime.Now;
        }

        public DnsServiceTargetSelectorReal(IDnsSrvQuerier dnsQuerier, int resultTtl)
        {
            DnsQuerier = dnsQuerier;
            ResultCacheTimeTtl = resultTtl;
            ResultFailTtl = resultTtl;
            ServerRecoveryTtl = DateTime.Now;
            Semaphore = new SemaphoreSlim(0, 1);
        }

        private bool ShouldRetrieveResult()
        {
            return QueryResult == null || !QueryResult.IsAvailable();
        }

        private async Task RetrieveQueryResultFromDns(DnsSrvServiceDescription service)
        {
            Semaphore.Release();
            if (ShouldRetrieveResult())
            {
                QueryResult = await DnsQuerier.QueryService(service, ResultCacheTimeTtl);
            }
        }

        public async Task<DnsEndPoint> SelectHost(DnsSrvServiceDescription service)
        {
            if (WaitForSeverRecovery())
            {
                return null;
            }

            if (ShouldRetrieveResult())
            {
                await RetrieveQueryResultFromDns(service);
            }

            var entryFound = QueryResult?.DnsEntries?.FirstOrDefault(entry => entry.IsAvailable());

            if (entryFound == null)
            {
                ServerRecoveryTtl = DateTime.Now.AddSeconds(ResultFailTtl);
                return null;
            }

            return entryFound.DnsEndPoint();
        }

        // Blacklist a host for some time. No questions asked.
        public void BlacklistHostFor(DnsEndPoint dnsHost, TimeSpan duration)
        {
            QueryResult.DnsEntries.ForEach(entry =>
            {
                if (entry.HostName == dnsHost.Host && entry.Port == dnsHost.Port)
                {
                    entry.PutInQuarantine(duration);
                }
            });
        }

        // Immediately remove a host from blacklist
        public void ResetBlacklistForHost(DnsEndPoint host)
        {
            QueryResult.DnsEntries.ForEach(entry => entry.ResetQuarantine());
        }

        // Most implementation (except mocks) will be stateful, a Reset() method will be handy.
        public void Reset()
        {
            ServerRecoveryTtl = DateTime.Now;
            QueryResult = null;
        }

        // In this class, you will implement logic to handle querying, retrying queries
        // to DNS if they fail, timing out the blacklisting, caching the query result for
        // some time, ...

        // You will NOT handle:
        //      - building URIs (here we don't know what an URI is
        //      - making decisions about what should be blacklisted
    }
}