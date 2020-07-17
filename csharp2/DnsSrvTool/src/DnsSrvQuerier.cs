namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    using DnsClient.Protocol;

    public class DnsSrvQuerier: IDnsSrvQuerier
    {
        private ILookupClient LookupClient { get; }

        public DnsSrvQuerier(ILookupClient lookupClient)
        {
            LookupClient = lookupClient;
        }

        internal List<DnsSrvResultEntry> ResolveServiceProcessResult(IDnsQueryResponse result)
        {
            // https://github.com/MichaCo/DnsClient.NET/blob/dev/src/DnsClient/DnsQueryExtensions.cs/#L628
            var hosts = new List<DnsSrvResultEntry>();
            if (result == null || result.HasError)
            {
                return hosts;
            }

            foreach (var entry in result.Answers.SrvRecords())
            {
                var timeToLive = entry.TimeToLive;
                var hostName = result.Additionals
                    .OfType<CNameRecord>()
                    .Where(p => p.DomainName.Equals(entry.Target))
                    .Select(p => p.CanonicalName).FirstOrDefault()
                    ?? entry.Target;

                hosts.Add(
                    new DnsSrvResultEntry(hostName, entry.Port,entry.Priority, entry.Weight, timeToLive)
                );
            }

            return hosts;
        }

        internal string CreateDnsQueryString(DnsSrvServiceDescription service)
        {
            return $"_{service.ServiceName}._{service.Protocol}.{service.Domain}.";
        }

        public async Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service)
        {
            string queryString = CreateDnsQueryString(service);
            var result = await LookupClient.QueryAsync(queryString, QueryType.SRV).ConfigureAwait(false);
            var queryResult = ResolveServiceProcessResult(result);
            return new DnsSrvQueryResult(queryResult);
        }
    }
}