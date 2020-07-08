namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    public class DnsServiceBalancingMessageHandler : DelegatingHandler
    {
        private DnsSrvServiceDescription ServiceDescription { get; }
        private IDnsServiceTargetSelector TargetSelector { get; }
        private ITargetQuarantinePolicy QuarantinePolicy { get; }
        private TimeSpan BlackListTime { get; } // TODO: set it

        public DnsServiceBalancingMessageHandler(
            DnsSrvServiceDescription serviceDescription,
            IDnsServiceTargetSelector targetSelector,
            ITargetQuarantinePolicy quarantinePolicy)
        {
            ServiceDescription = serviceDescription;
            TargetSelector = targetSelector;
            QuarantinePolicy = quarantinePolicy;
        }

        private Uri ReplaceHost(Uri original, DnsEndPoint newHost)
        {
            if (newHost == null)
            {
                return original;
            }

            var builder = new UriBuilder(original)
            {
                Host = newHost.Host,
                Port = newHost.Port,
            };

            return builder.Uri;
        }

        /// <summary>
        /// SendAsync override method.
        /// </summary>
        /// <param name="request">The request given.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns>The Http response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            while (true)
            {
                DnsEndPoint host = await TargetSelector.SelectHost(ServiceDescription);
                request.RequestUri = ReplaceHost(request.RequestUri, host);
                var response = await base.SendAsync(request, cancellationToken);
                if (host != null && QuarantinePolicy.ShouldQuarantine(response))
                {
                    TargetSelector.BlacklistHostFor(host, BlackListTime);
                }
                else
                {
                    return response;
                }
            }
        }
    }
}