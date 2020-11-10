namespace DnsSrvTool.Test
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307,

    public class HandlerWrapper : DelegatingHandler
    {
        public async Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await SendAsync(request, cancellationToken);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken);
        }
    }
}