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
    using Microsoft.Extensions.Logging;
    using DnsClient;
    using NLog;
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
        public void CreateABuildCheckTypeValues()
        {
            uint cacheTime = 20;
            uint retrieveTime = 20;
            ProtocolType protocol = ProtocolType.Tcp;
            string uriString = "https://api.qarnot.com";
            Uri uri = new Uri(uriString);
            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(protocol);
            ILookupClient dnsClient = new LookupClient();
            IDnsSrvQuerier querier = new DnsSrvQuerier(dnsClient); // extract sort elements !
            DnsSrvServiceDescription service = extract.FromUri(uri);
            IDnsServiceTargetSelector selector = new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), cacheTime, retrieveTime);
            ITargetQuarantinePolicy quarantinePolice = new TargetQuarantinePolicyServeurUnavailable();
            using var delegateHandler = new DnsServiceBalancingMessageHandler(service, selector, quarantinePolice, null);
        }

        public HandlerWrapper WrapDnsHandler(DelegatingHandler dnsHandler, string successResponse)
        {
            if (dnsHandler == null)
            {
                throw new ArgumentNullException(nameof(dnsHandler));
            }

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
            var logger = CreateILoggerFromNLog();
            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(), logger);
            using HandlerWrapper handlerWrapper = WrapDnsHandler(dnsHandler, "responseSuccess");

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
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, HttpStatusCode.Accepted };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(), null);

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
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(), null);

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
            var logger = CreateILoggerFromNLog();
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10, logger), new TargetQuarantinePolicyServeurUnavailable(new TimeSpan(0, 0, 10)), logger);

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

            Assert.AreEqual("responseSuccess", content);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            Assert.AreEqual("hello.world.com", handler.UrlCall.Host);
        }

        [Test]
        public async Task LaunchErrorRequestAndWaitForRetrieveQarantaine()
        {
            FakeHTTPHandler handler = new FakeHTTPHandler();
            handler.ReturnMessage = "responseSuccess";
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.InternalServerError };

            IDnsServiceExtractor extract = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            FakeDnsSrvQuerier querier = new FakeDnsSrvQuerier();
            var dnsHandler = new DnsServiceBalancingMessageHandler(extract.FromUri(new Uri("https://api.qarnot.com")), new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), 20, 10), new TargetQuarantinePolicyServeurUnavailable(new TimeSpan(0, 0, 10)), null);

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
            handler.ReturnStatusCodeList = new List<HttpStatusCode>() { HttpStatusCode.Accepted };
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

        private Microsoft.Extensions.Logging.ILogger CreateILoggerFromNLog(bool debug = false)
        {
            // Example of NLog build
            // https://stackoverflow.com/questions/56534730/nlog-works-in-asp-net-core-app-but-not-in-net-core-xunit-test-project
            // NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
            var configuration = new NLog.Config.LoggingConfiguration();
            configuration.AddRuleForAllLevels(new NLog.Targets.ConsoleTarget());
            NLog.Web.NLogBuilder.ConfigureNLog(configuration);

            // Create provider to bridge Microsoft.Extensions.Logging
            var provider = new NLog.Extensions.Logging.NLogLoggerProvider();

            // Create logger
            Microsoft.Extensions.Logging.ILogger logger = provider.CreateLogger(typeof(DnsSrvToolsIntergrationTest).FullName);

            // ILogger logger = NLog.LogManager.GetCurrentClassLogger();
            if (debug)
            {
                logger.LogDebug("This is a test of the log system : LogDebug.");
                logger.LogTrace("This is a test of the log system : LogTrace.");
                logger.LogInformation("This is a test of the log system : LogInformation.");
                logger.LogWarning("This is a test of the log system : LogWarning.");
                logger.LogError("This is a test of the log system : LogError.");
                logger.LogCritical("This is a test of the log system : LogCritical.");
            }

            return logger;
        }
    }
}