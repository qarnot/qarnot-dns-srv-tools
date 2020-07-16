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
            IDnsServiceTargetSelector selector = new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), cacheTime, retrieveTime);
            ITargetQuarantinePolicy quarantinePolice = new TargetQuarantinePolicyServeurUnavailable();
            var delegateHandler = new DnsServiceBalancingMessageHandler(service, selector, quarantinePolice);
        }

        public HandlerWrapper wrapDnsHandler(DelegatingHandler dnsHandler, string successResponse)
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = successResponse;
            // add the fake handle
            dnsHandler.InnerHandler = handler;
            // wrapper used to send the chosen request
            HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;
            return handlerWrapper;
        }

        [Test]
        public async Task LaunchASimpleRequestMustSuccess()
        {
            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable());
            using HandlerWrapper handlerWrapper = wrapDnsHandler(dnsHandler, "responseSuccess");

            // create the request
            using var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");
            // get the result
            var result = await handlerWrapper.Send(requestMessage, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(content, "responseSuccess");
        }

        [Test]
        public async Task LaunchASimpleRequestWithQarantainValuesMustSuccess()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>(){ HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, HttpStatusCode.Accepted };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable());
            // add the fake handle
            dnsHandler.InnerHandler = handler;
            // wrapper used to send the chosen request
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;

            // create the request
            using var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");
            // get the result
            var result = await handlerWrapper.Send(requestMessage, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();

            Assert.AreEqual("responseSuccess", content);
            Assert.AreEqual(HttpStatusCode.Accepted, result.StatusCode);
            Assert.AreEqual(querier.DnsSrvResultEntryList[3].HostName, handler.UrlCall.Host);
        }

        [Test]
        public async Task LaunchASimpleRequestWithErrorOnAllTheServerMustReturnAnErrorResponse()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>(){ HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable());
            // add the fake handle
            dnsHandler.InnerHandler = handler;
            // wrapper used to send the chosen request
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;

            // create the request
            using var requestMessage = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            // get the result
            var result = await handlerWrapper.Send(requestMessage, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
        }


        [Test]
        public async Task LaunchErrorUseTheOriginalUriIfNoDnsServerWork()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(new TimeSpan(0, 0, 10)));
            // add the fake handle
            dnsHandler.InnerHandler = handler;
            // wrapper used to send the chosen request
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;

            // create the request
            using var requestMessage1 = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            // get the result
            var result = await handlerWrapper.Send(requestMessage1, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreEqual("hello.world.com", handler.UrlCall.Host);
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.Accepted };
            using var requestMessage2 = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            result = await handlerWrapper.Send(requestMessage2, default(CancellationToken));
            content = await result.Content.ReadAsStringAsync();

            // Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual("responseSuccess", content);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            Assert.AreEqual("hello.world.com", handler.UrlCall.Host);
        }

        [Test]
        public async Task LaunchErrorRequestAndWaitForRetrieveQarantaine()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>(){ HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(new TimeSpan(0, 0, 10)));
            // add the fake handle
            dnsHandler.InnerHandler = handler;
            // wrapper used to send the chosen request
            using HandlerWrapper handlerWrapper = new HandlerWrapper();
            handlerWrapper.InnerHandler = dnsHandler;

            // create the request
            using var requestMessage1 = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            // get the result
            var result = await handlerWrapper.Send(requestMessage1, default(CancellationToken));
            var content = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
            handler.ReturnStatusCodeList = new List<HttpStatusCode>(){ HttpStatusCode.Accepted };
            using var requestMessage2 = new HttpRequestMessage(new HttpMethod("Get"), "https://hello.world.com");

            result = await handlerWrapper.Send(requestMessage2, default(CancellationToken));
            content = await result.Content.ReadAsStringAsync();

            // Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual("responseSuccess", content);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            Assert.AreEqual("hello.world.com", handler.UrlCall.Host);
            await Task.Delay(10100);
            result = await handlerWrapper.Send(requestMessage2, default(CancellationToken));
            content = await result.Content.ReadAsStringAsync();
            // Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual("responseSuccess", content);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            Assert.AreEqual(querier.DnsSrvResultEntryList[0].HostName, handler.UrlCall.Host);
        }
    }
}