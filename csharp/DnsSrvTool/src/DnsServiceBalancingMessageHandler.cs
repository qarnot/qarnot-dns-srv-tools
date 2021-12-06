#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// DelegatingHandler used to get the dsn srv hostnames
    /// and use them to do the called request.
    /// </summary>
    public class DnsServiceBalancingMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServiceBalancingMessageHandler"/> class.
        /// </summary>
        /// <param name="serviceDescription">The server description.</param>
        /// <param name="targetSelector">The api caller and selector.</param>
        /// <param name="quarantinePolicy">The respose quarantine policy to blacklist an host and retrieve the request.</param>
        /// <param name="logger">The logger.</param>
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

        private DnsSrvServiceDescription ServiceDescription { get; }
        private IDnsServiceTargetSelector TargetSelector { get; }
        private ITargetQuarantinePolicy QuarantinePolicy { get; }
        private ILogger Logger { get; }

        /// <summary>
        /// SendAsync override method.
        /// Recursive call.
        /// </summary>
        /// <param name="request">The request given.</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns>The Http response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Uri originalUri = request.RequestUri;
            DnsEndPoint host = await TargetSelector.SelectHostAsync(ServiceDescription);
            if (host == null)
            {
                Logger?.LogInformation("No Dns Host Found");
                return await base.SendAsync(request, cancellationToken);
            }

            request.RequestUri = ReplaceHost(request.RequestUri, host);
            Logger?.LogInformation("Request uri : {requestRequestUri}", request.RequestUri);
            var response = await base.SendAsync(request, cancellationToken);
            if (response == null)
            {
                return response;
            }

            Logger?.LogTrace("Response status code : {response.StatusCode}", response.StatusCode);
            if (QuarantinePolicy.ShouldQuarantine(response))
            {
                Logger?.LogWarning("Host {host} (from original host {original_host}) is send in quarantine", host, originalUri.Host);
                await TargetSelector.BlacklistHostForAsync(host, QuarantinePolicy.QuarantineDuration);
                request.RequestUri = originalUri;
                return await SendAsync(request, cancellationToken);
            }

            return response;
        }

        private Uri ReplaceHost(Uri original, DnsEndPoint newHost)
        {
            Logger?.LogTrace("ReplaceHost: Replace the Hostname: {originalHost}:{originalPort} by the new HostName: {newHostHost}:{newHostPort}", original.Host, original.Port, newHost.Host, newHost.Port);
            var builder = new UriBuilder(original)
            {
                Host = newHost.Host.Trim('.'),
                Port = newHost.Port,
            };

            return builder.Uri;
        }
    }
}
