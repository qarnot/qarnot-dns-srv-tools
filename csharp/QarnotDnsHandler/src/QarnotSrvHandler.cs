namespace QarnotDnsHandler
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1054, CA1822

    /// <summary>
    /// The qarnot handler to get the qarnot api addresses from the dns.
    /// </summary>
    public class QarnotSrvHandler : DelegatingHandler
    {
        private string BaseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="QarnotSrvHandler"/> class.
        /// </summary>
        /// <param name="uri">the base url.</param>
        /// <param name="dnsCacheTime">the cachetime before refreshing the addresses list.</param>
        public QarnotSrvHandler(string uri, int? dnsCacheTime = null, int? dnsFailTime = null, int? dnsRetrieveTime = null, string service = null, string domain = null, ProtocolType? protocol = null)
        {
            BaseUri = uri;

            service = service ?? QEnvVariables.DnsService;
            domain = domain ?? QEnvVariables.DnsDomain;
            protocol = protocol ?? QEnvVariables.DnsProtocol;
            var serviceValue = service ?? DnsResolver.DEFAULT_SERVICE;
            var domainValue = domain ?? DnsResolver.DEFAULT_DOMAIN;
            var protocolValue = protocol ?? DnsResolver.DEFAULT_PROTOCOL;
            DnsUrlResolver = new DnsResolver(uri, serviceValue, domainValue, protocolValue);

            dnsCacheTime = dnsCacheTime ?? QEnvVariables.DnsCachetime;
            dnsFailTime = dnsFailTime ?? QEnvVariables.DnsFailTime;
            dnsRetrieveTime = dnsRetrieveTime ?? QEnvVariables.DnsRetrieve;
            var cachetime = dnsCacheTime ?? DnsSrvManager.DEFAULT_CACHETIME;
            var failtime = dnsFailTime ?? DnsSrvManager.DEFAULT_FAILTIME;
            var retrievetime = dnsRetrieveTime ?? DnsSrvManager.DEFAULT_RETRIEVETIME;
            DnsSrvUriGetter = new DnsSrvManager(DnsUrlResolver, cachetime, failtime, retrievetime);
        }

        private IDnsSrvManager DnsSrvUriGetter { get; }
        private DnsResolver DnsUrlResolver { get; }

        /// <summary>
        /// SendAsync override method.
        /// </summary>
        /// <param name="request">The request given.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns>The Http response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string pathUri = request?.RequestUri?.ToString().Replace(BaseUri, string.Empty);

            while (true)
            {
                // change the uri if needed
                await DnsSrvUriGetter.BalanceApiServerUri(cancellationToken);
                var requestUri = DnsSrvUriGetter.GetUri(pathUri);
                if (requestUri != null)
                {
                    request.RequestUri = requestUri;
                }

                // get the response
                var response = await base.SendAsync(request, cancellationToken);

                // return the response if it is good
                if (AvailableServer(response))
                {
                    return response;
                }

                // or change the uri used
                if (DnsSrvUriGetter.NextApiUri() == false)
                    return response;
            }
        }

        private bool AvailableServer(HttpResponseMessage response)
        {
            return !DnsUrlResolver.DnsSrvMatch || !ServerUnavailable(response);
        }

        /// <summary>
        /// Check the server availability of the return response.
        /// </summary>
        /// <para>Response given by the sever.</para>
        /// <returns>Is server unavailable.</returns>
        private bool ServerUnavailable(HttpResponseMessage response)
        {
            var unavailableStatus = new List<System.Net.HttpStatusCode>()
            {
                System.Net.HttpStatusCode.InternalServerError,
                System.Net.HttpStatusCode.BadGateway,
                System.Net.HttpStatusCode.GatewayTimeout,
                System.Net.HttpStatusCode.ServiceUnavailable,
            };

            return unavailableStatus.Contains(response.StatusCode);
        }
    }
}