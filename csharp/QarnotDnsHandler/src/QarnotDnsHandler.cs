namespace QarnotDsnHandler
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class DsnSrvHandler : DelegatingHandler
    {
        public virtual IGetUri DnsSrvUriGetter { get; }

        private string BaseUri;

        public DsnSrvHandler(string uri, int dnsCacheTime)
        {
            BaseUri = uri;
            DnsSrvUriGetter = new GetDnsSrv(uri, dnsCacheTime);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string pathUri = request.RequestUri.ToString().Replace(BaseUri, "");

            while (true)
            {
                // change the uri if needed
                await DnsSrvUriGetter.BalanceApiServerUri(cancellationToken);
                request.RequestUri = DnsSrvUriGetter.GetUri(pathUri);

                // get the response
                var response = await base.SendAsync(request, cancellationToken);

                // return the response if it is good
                if (AvailableServer(response, cancellationToken))
                {
                    return response;
                }

                // or change the uri used
                await DnsSrvUriGetter.NextApiUri(cancellationToken);
            }
        }

        private bool AvailableServer(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return (!DnsSrvUriGetter.DnsSrvFind || !LookForUnavailable(response, cancellationToken));
        }

        protected bool LookForUnavailable(HttpResponseMessage response,
            CancellationToken ct = default(CancellationToken))
        {
            var unavailableStatus = new List<System.Net.HttpStatusCode>()
            {
                System.Net.HttpStatusCode.InternalServerError,
                System.Net.HttpStatusCode.BadGateway,
                System.Net.HttpStatusCode.GatewayTimeout,
                System.Net.HttpStatusCode.ServiceUnavailable,
            };

            return (unavailableStatus.Contains(response.StatusCode));
        }
    }
}