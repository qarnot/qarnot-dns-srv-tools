namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307, CA2000, CA1054

    [TestFixture]
    public class DnsServiceExtractorFirstLabelConventionTester
    {
        [TestCase("https://api.qarnot.com", "api", "qarnot.com")]
        [TestCase("https://api.dev.qarnot.com", "api", "dev.qarnot.com")]
        [TestCase("https://api.test.qarnot.com", "api", "test.qarnot.com")]
        [TestCase("https://test.qarnot.com", "test", "qarnot.com")]
        [TestCase("https://api.qarnot.retribution.fr", "api", "qarnot.retribution.fr")]
        [TestCase("https://api.test.random.domain.with.test.com", "api", "test.random.domain.with.test.com")]
        [Test]
        public void TestDifferentFromUri(string url, string service, string domain)
        {
            Uri uri = new Uri(url);
            IDnsServiceExtractor extractor = new DnsServiceExtractorFirstLabelConvention(null);
            DnsSrvServiceDescription description = extractor.FromUri(uri);
            Assert.AreEqual(service, description.ServiceName);
            Assert.AreEqual(domain, description.Domain);
        }

        [TestCase("https://api.qarnot.com", "api", "qarnot.com", true)]
        [TestCase("https://api.dev.qarnot.com", "api", "dev.qarnot.com", true)]
        [TestCase("https://api.test.qarnot.com", "api", "test.qarnot.com", false)]
        [TestCase("https://api.test.qarnot.com", "hello", "test.qarnot.com", false)]
        [TestCase("https://test.qarnot.com", "test", "qarnot.com", true)]
        [TestCase("https://api.qarnot.retribution.fr", "api", "qarnot.retribution.fr", true)]
        [TestCase("https://api.test.random.domain.with.test.com", "api", "test.random.domain.with.test.com", false)]
        [Test]
        public void TestDifferentFromUriWithServiceAndDomainWhiteList(string url, string service, string domain, bool isTrue)
        {
            Uri uri = new Uri(url);
            var serviceWhiteList = new List<string>() { "api", "test" };
            var domainWhiteList = new List<string>() { "qarnot.com", "dev.qarnot.com", "qarnot.retribution.fr" };
            IDnsServiceExtractor extractor = new DnsServiceExtractorFirstLabelConvention(null, serviceWhiteList, domainWhiteList);
            DnsSrvServiceDescription description = extractor.FromUri(uri);
            if (isTrue)
            {
                Assert.AreEqual(service, description.ServiceName);
                Assert.AreEqual(domain, description.Domain);
            }
            else
            {
                Assert.AreEqual(null, description);
                Assert.AreEqual(null, description);
            }
        }

        [TestCase("https://api.qarnot.com", "api", "qarnot.com", true)]
        [TestCase("https://api.dev.qarnot.com", "api", "dev.qarnot.com", true)]
        [TestCase("https://api.test.qarnot.com", "api", "test.qarnot.com", true)]
        [TestCase("https://api.hello.long.tentation.test.qarnot.com", "api", "hello.long.tentation.test.qarnot.com", true)]
        [TestCase("https://api.test.qarnot.fr", "api", "test.qarnot.fr", false)]
        [TestCase("https://api.test.qarnot.com.false", "api", "test.qarnot.com.false", false)]
        [TestCase("https://hello.test.qarnot.com", "hello", "test.qarnot.com", false)]
        [TestCase("https://test.qarnot.com", "test", "qarnot.com", true)]
        [TestCase("https://api.qarnot.retribution.fr", "api", "qarnot.retribution.fr", true)]
        [TestCase("https://api.test.random.domain.with.test.com", "api", "test.random.domain.with.test.com", false)]
        [Test]
        public void TestDifferentFromUriWithServiceAndDomainWhiteListWithAllowSubDomain(string url, string service, string domain, bool isTrue)
        {
            Uri uri = new Uri(url);
            var serviceWhiteList = new List<string>() { "api", "test" };
            var domainWhiteList = new List<string>() { "qarnot.com", "qarnot.retribution.fr" };
            IDnsServiceExtractor extractor = new DnsServiceExtractorFirstLabelConvention(null, serviceWhiteList, domainWhiteList, true);
            DnsSrvServiceDescription description = extractor.FromUri(uri);
            if (isTrue)
            {
                Assert.AreEqual(service, description.ServiceName);
                Assert.AreEqual(domain, description.Domain);
            }
            else
            {
                Assert.AreEqual(null, description);
                Assert.AreEqual(null, description);
            }
        }

        [TestCase(ProtocolType.Tcp)]
        [TestCase(ProtocolType.Udp)]
        [TestCase(ProtocolType.Ipx)]
        [TestCase(ProtocolType.Raw)]
        [TestCase(ProtocolType.Idp)]
        [TestCase(ProtocolType.Ggp)]
        [TestCase(ProtocolType.IPv6)]
        [Test]
        public void TestDifferentFromUriWithServiceAndDomainWhiteList(ProtocolType protocol)
        {
            IDnsServiceExtractor extractor = new DnsServiceExtractorFirstLabelConvention(protocol);
            DnsSrvServiceDescription description = extractor.FromUri(new Uri("https://api.qarnot.com"));
            Assert.AreEqual(description.Protocol, protocol);
        }
    }
}
