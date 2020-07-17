namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    using Microsoft.Extensions.Logging;

    public class DnsServiceBalancingMessageHandler : DelegatingHandler
    {
        private DnsSrvServiceDescription ServiceDescription { get; }
        private IDnsServiceTargetSelector TargetSelector { get; }
        private ITargetQuarantinePolicy QuarantinePolicy { get; }
        private ILogger Logger { get; }

        public DnsServiceBalancingMessageHandler(
            DnsSrvServiceDescription serviceDescription,
            IDnsServiceTargetSelector targetSelector,
            ITargetQuarantinePolicy quarantinePolicy,
            ILogger logger = null)
        {
            ServiceDescription = serviceDescription;
            TargetSelector = targetSelector;
            QuarantinePolicy = quarantinePolicy;
            Logger = logger;
        }

        private Uri ReplaceHost(Uri original, DnsEndPoint newHost)
        {
            if (newHost == null)
            {
                return original;
            }

            Logger?.LogInformation($"ReplaceHost: OldHost: {original.Host}:{original.Port} NewHost: {newHost.Host}:{newHost.Port}");
            var builder = new UriBuilder(original)
            {
                Host = newHost.Host,
                Port = newHost.Port,
            };

            return builder.Uri;
        }

        /// <summary>
        /// SendAsync override method.
        /// Recursive call
        /// </summary>
        /// <param name="request">The request given.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns>The Http response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri originalUri = request.RequestUri;
            DnsEndPoint host = await TargetSelector.SelectHostAsync(ServiceDescription);
            if (host == null)
            {
                Logger?.LogInformation($"No Dns Host Found");
                return await base.SendAsync(request, cancellationToken);
            }

            request.RequestUri = ReplaceHost(request.RequestUri, host);
            var response = await base.SendAsync(request, cancellationToken);
            Logger?.LogInformation($"Response status code : {response.StatusCode}");
            if (QuarantinePolicy.ShouldQuarantine(response))
            {
                Logger?.LogInformation($"Host {host} is send in quarantine");
                TargetSelector.BlacklistHostFor(host, QuarantinePolicy.QuarantineDuration);
                request.RequestUri = originalUri;
                return await SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}