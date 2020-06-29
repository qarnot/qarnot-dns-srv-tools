namespace QarnotDsnHandler.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    public class FakeLookClient : ILookupClient, IDnsQuery
    {
        public List<ServiceHostEntry> ServiceList { get; set; } = null;

        public IReadOnlyCollection<NameServer> NameServers { get; set; }

        public LookupClientSettings Settings { get; set; }

        public TimeSpan? MinimumCacheTimeout { get; set; }

        public bool EnableAuditTrail { get; set; }

        public bool UseCache { get; set; }

        public bool Recursion { get; set; }

        public int Retries { get; set; }

        public bool ThrowDnsErrors { get; set; }

        public bool UseRandomNameServer { get; set; }

        public bool ContinueOnDnsError { get; set; }

        public TimeSpan Timeout { get; set; }

        public bool UseTcpFallback { get; set; }

        public bool UseTcpOnly { get; set; }

        public IDnsQueryResponse Query(string a, QueryType b, QueryClass c)
        {
            throw new NotImplementedException("fake Look Query Client");
        }

        public IDnsQueryResponse Query(DnsQuestion a)
        {
            throw new NotImplementedException("fake Look Query Client");
        }

        public IDnsQueryResponse Query(DnsQuestion a, DnsQueryAndServerOptions b)
        {
            throw new NotImplementedException("fake Look Query Client");
        }

        public Task<IDnsQueryResponse> QueryAsync(string a, QueryType b, QueryClass c, CancellationToken d)
        {
            throw new NotImplementedException("fake Look  QueryAsync 1 Client");
        }

        public Task<IDnsQueryResponse> QueryAsync(DnsQuestion a, CancellationToken b)
        {
            throw new NotImplementedException("fake Look  QueryAsync 2 Client");
        }

        public Task<IDnsQueryResponse> QueryAsync(DnsQuestion a, DnsQueryAndServerOptions b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryAsync 3 Client");
        }

        public IDnsQueryResponse QueryReverse(IPAddress a)
        {
            throw new NotImplementedException("fake Look QueryReverse Client");
        }

        public IDnsQueryResponse QueryReverse(IPAddress a, DnsQueryAndServerOptions c)
        {
            throw new NotImplementedException("fake Look QueryReverse Client");
        }

        public Task<IDnsQueryResponse> QueryReverseAsync(IPAddress a, CancellationToken b)
        {
            throw new NotImplementedException("fake Look  QueryReverseAsync Client");
        }

        public Task<IDnsQueryResponse> QueryReverseAsync(IPAddress a, DnsQueryAndServerOptions b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryReverseAsync Client");
        }

        public IDnsQueryResponse QueryServer(IReadOnlyCollection<NameServer> a, string b, QueryType c, QueryClass d)
        {
            throw new NotImplementedException("fake Look QueryServer Client");
        }

        public IDnsQueryResponse QueryServer(IReadOnlyCollection<NameServer> a, DnsQuestion b)
        {
            throw new NotImplementedException("fake Look QueryServer Client");
        }

        public IDnsQueryResponse QueryServer(IReadOnlyCollection<NameServer> a, DnsQuestion b, DnsQueryOptions c)
        {
            throw new NotImplementedException("fake Look QueryServer Client");
        }

        public IDnsQueryResponse QueryServer(IReadOnlyCollection<IPEndPoint> a, string b, QueryType c, QueryClass d)
        {
            throw new NotImplementedException("fake Look QueryServer Client");
        }

        public IDnsQueryResponse QueryServer(IReadOnlyCollection<IPAddress> a, string b, QueryType c, QueryClass d)
        {
            throw new NotImplementedException("fake Look QueryServer Client");
        }

        public Task<IDnsQueryResponse> QueryServerAsync(IReadOnlyCollection<NameServer> a, string b, QueryType c, QueryClass d, CancellationToken e)
        {
            throw new NotImplementedException("fake Look  QueryServerAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerAsync(IReadOnlyCollection<NameServer> a, DnsQuestion b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryServerAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerAsync(IReadOnlyCollection<NameServer> a, DnsQuestion b, DnsQueryOptions c, CancellationToken d)
        {
            throw new NotImplementedException("fake Look  QueryServerAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerAsync(IReadOnlyCollection<IPAddress> a, string b, QueryType c, QueryClass d, CancellationToken e)
        {
            throw new NotImplementedException("fake Look  QueryServerAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerAsync(IReadOnlyCollection<IPEndPoint> a, string b, QueryType c, QueryClass d, CancellationToken e)
        {
            throw new NotImplementedException("fake Look  QueryServerAsync Client");
        }

        public IDnsQueryResponse QueryServerReverse(IReadOnlyCollection<IPAddress> a, IPAddress b)
        {
            throw new NotImplementedException("fake Look QueryServerReverse Client");
        }

        public IDnsQueryResponse QueryServerReverse(IReadOnlyCollection<IPEndPoint> a, IPAddress b)
        {
            throw new NotImplementedException("fake Look QueryServerReverse Client");
        }

        public IDnsQueryResponse QueryServerReverse(IReadOnlyCollection<NameServer> a, IPAddress b)
        {
            throw new NotImplementedException("fake Look QueryServerReverse Client");
        }

        public IDnsQueryResponse QueryServerReverse(IReadOnlyCollection<NameServer> a, IPAddress b, DnsQueryOptions c)
        {
            throw new NotImplementedException("fake Look QueryServerReverse Client");
        }

        public Task<IDnsQueryResponse> QueryServerReverseAsync(IReadOnlyCollection<IPAddress> a, IPAddress b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryServerReverseAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerReverseAsync(IReadOnlyCollection<IPEndPoint> a, IPAddress b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryServerReverseAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerReverseAsync(IReadOnlyCollection<NameServer> a, IPAddress b, CancellationToken c)
        {
            throw new NotImplementedException("fake Look  QueryServerReverseAsync Client");
        }

        public Task<IDnsQueryResponse> QueryServerReverseAsync(IReadOnlyCollection<NameServer> a, IPAddress b, DnsQueryOptions c, CancellationToken d)
        {
            throw new NotImplementedException("fake Look  QueryServerReverseAsync Client");
        }
    }
}
