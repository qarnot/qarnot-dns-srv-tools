#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DnsClient;
    using DnsClient.Protocol;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Dns srv querier
    /// use an ILookupClient to do a srv call.
    /// </summary>
    public class DnsSrvQuerier : IDnsSrvQuerier
    {
        private ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvQuerier"/> class with a default lookup client.
        /// </summary>
        /// <param name="logger">Optional Logger.</param>
        public DnsSrvQuerier(ILogger logger = null)
            : this(new LookupClient(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvQuerier"/> class.
        /// </summary>
        /// <param name="lookupClient">DnsClient object to do the DNS SRV calls.</param>
        /// <param name="logger">Optional Logger.</param>
        public DnsSrvQuerier(ILookupClient lookupClient, ILogger logger = null)
        {
            LookupClient = lookupClient;
            Logger = logger;
        }

        private ILookupClient LookupClient { get; }

        /// <summary>
        /// Ask a new srv call.
        /// </summary>
        /// <param name="service">Address to be call.</param>
        /// <returns>Call Response with HostName, Port, Priority, Weight and Ttl.</returns>
        public async Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service)
        {
            if (service == null)
            {
                Logger?.LogError("DnsSrvServiceDescription cannot be null.");
                throw new ArgumentNullException(nameof(service));
            }

            string queryString = CreateDnsQueryString(service);
            var result = await LookupClient.QueryAsync(queryString, QueryType.SRV).ConfigureAwait(false);
            var queryResult = ResolveServiceProcessResult(result);
            return new DnsSrvQueryResult(queryResult);
        }

        /// <summary>
        /// Extract the IDnsQueryResponse to build a List of DnsSrvResultEntry.
        /// </summary>
        /// <param name="result">Query response.</param>
        /// <returns>Entities List.</returns>
        protected List<DnsSrvResultEntry> ResolveServiceProcessResult(IDnsQueryResponse result)
        {
            // https://github.com/MichaCo/DnsClient.NET/blob/dev/src/DnsClient/DnsQueryExtensions.cs/#L628
            var hosts = new List<DnsSrvResultEntry>();
            if (result == null || result.HasError)
            {
                var errorMessage = result == null ? "Dns request return a null response" : result.ErrorMessage;
                Logger?.LogWarning("Dns Request fail: {errorMessage} returning empty hosts list", errorMessage);
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

                var dnsEntry = new DnsSrvResultEntry(hostName, entry.Port, entry.Priority, entry.Weight, timeToLive);
                Logger?.LogDebug("Dns Entry create : {dnsEntry}", dnsEntry.ToString("f"));
                hosts.Add(dnsEntry);
            }

            return hosts;
        }

        /// <summary>
        /// Create a Dns url string from the DnsSrvServiceDescription.
        /// </summary>
        /// <param name="service">The dns info.</param>
        /// <returns>Dns url create.</returns>
        protected string CreateDnsQueryString(DnsSrvServiceDescription service)
        {
            service = service ?? throw new ArgumentNullException(nameof(service), "The service should not be null.");
            string dnsQueryString = $"_{service.ServiceName}._{service.Protocol}.{service.Domain}.";
            Logger?.LogDebug("Dns query string build : {dnsQueryString}" + dnsQueryString);
            return dnsQueryString;
        }
    }
}