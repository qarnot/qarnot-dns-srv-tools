namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    public class DnsSrvQuerier: IDnsSrvQuerier
    {
        private ILookupClient LookupClient { get; }

        public DnsSrvQuerier(ILookupClient lookupClient)
        {
            LookupClient = lookupClient;
        }

        public async Task<DnsSrvQueryResult> QueryService(DnsSrvServiceDescription service, int resultLifeTime)
        {
            var dnsServicesHost = await LookupClient.ResolveServiceAsync(service.Domain, service.ServiceName, service.Protocol);
            var queryResult = dnsServicesHost.Select(serviceHost => new DnsSrvResultEntry(serviceHost.HostName, serviceHost.Port, serviceHost.Priority, serviceHost.Weight, resultLifeTime)).ToList();
            return new DnsSrvQueryResult(queryResult, resultLifeTime);
        }
    }
}