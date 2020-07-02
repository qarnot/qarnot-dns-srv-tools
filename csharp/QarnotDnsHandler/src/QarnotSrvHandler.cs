namespace QarnotDnsHandler
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1054, CA1822

    /// <summary>
    /// The qarnot handler to get the qarnot api addresses from the dns.
    /// </summary>
    public class QarnotSrvHandler : DelegatingHandler
    {
        private const int DefaultDnsCachetime = 5;

        private string BaseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="QarnotSrvHandler"/> class.
        /// </summary>
        /// <param name="uri">the base url.</param>
        /// <param name="dnsCacheTime">the cachetime before refreshing the addresses list.</param>
        public QarnotSrvHandler(string uri, int? dnsCacheTime)
        {
            BaseUri = uri;
            DnsSrvUriGetter = dnsCacheTime.HasValue ?
                            new GetDnsSrv(uri, dnsCacheTime.Value) :
                            new GetDnsSrv(uri, DefaultDnsCachetime);
        }

        private IGetDnsSrv DnsSrvUriGetter { get; }

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
                request.RequestUri = DnsSrvUriGetter.GetUri(pathUri);

                // get the response
                var response = await base.SendAsync(request, cancellationToken);

                // return the response if it is good
                if (AvailableServer(response))
                {
                    return response;
                }

                // or change the uri used
                await DnsSrvUriGetter.NextApiUri(cancellationToken);
            }
        }

        private bool AvailableServer(HttpResponseMessage response)
        {
            return !DnsSrvUriGetter.DnsSrvFind || !ServerUnavailable(response);
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