namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    using NUnit.Framework;

    [TestFixture]
    public class DnsSrvToolsIntergrationTest
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
        public async Task CreateABuildCheckTypeValues()
        {
            int cacheTime = 20;
            int retrieveTime = 20;
            int quarantineTime = 20;
            ProtocolType protocol = ProtocolType.Tcp;
            string uriString = "https://api.qarnot.com";
            Uri uri = new Uri(uriString);
            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(protocol);
            ILookupClient dnsClient = new LookupClient();
            IDnsSrvQuerier querier = new DnsSrvQuerier(dnsClient); // extract sort elements !
            DnsSrvServiceDescription service = extract.FromUri(uri);
            IDnsServiceTargetSelector selector = new DnsServiceTargetSelectorReal(querier, cacheTime);
            ITargetQuarantinePolicy quarantinePolice = new TargetQuarantinePolicyServeurUnavailable();
            var delegateHandler = new DnsServiceBalancingMessageHandler(service, selector, quarantinePolice);
        }

        [Test]
        public async Task LaunchASimpleRequest()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, 20), new TargetQuarantinePolicyServeurUnavailable());

            dnsHandler.InnerHandler = handler;
            using var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;
            var result = await handlerWrapper.Send(requestMessage, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();
            Assert.AreEqual(content, "responseSuccess");
        }

        public class FakeDnsSrvQuerier : IDnsSrvQuerier
        {
            public async Task<DnsSrvQueryResult> QueryService(DnsSrvServiceDescription service, int resultLifeTime)
            {
                var DnsSrvResultEntryList =  new List<DnsSrvResultEntry>()
                {
                    new DnsSrvResultEntry("api.qarnot1.com", 430, 4, 10, 20),
                    new DnsSrvResultEntry("api.qarnot1.com", 430, 6, 10, 20),
                    new DnsSrvResultEntry("api.qarnot1.com", 430, 1, 10, 20),
                    new DnsSrvResultEntry("api.qarnot1.com", 430, 3, 10, 20),
                    new DnsSrvResultEntry("api.qarnot1.com", 430, 2, 10, 20),
                };
                return new DnsSrvQueryResult(DnsSrvResultEntryList, resultLifeTime);
            }
        }

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
}