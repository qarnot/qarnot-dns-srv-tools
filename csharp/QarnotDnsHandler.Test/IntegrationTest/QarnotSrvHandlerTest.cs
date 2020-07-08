namespace QarnotDnsHandler.Test
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
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303, CA1054

    [TestFixture]
    public class QarnotSrvHandlerTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task TestNextApiUri()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            var srvHandler = new QarnotSrvHandler("https://api.qarnot.com");
            srvHandler.InnerHandler = handler;
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = srvHandler;
            using var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            var result = await handlerWrapper.Send(requestMessage, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();
            Assert.AreEqual(content, "responseSuccess");
        }

        private class HandlerWrapper : DelegatingHandler
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
}