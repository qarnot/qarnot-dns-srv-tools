namespace QarnotDnsHandler.Test
{
    using System.Net.Sockets;
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303

    [TestFixture]
    public class DnsResolverTest
    {
        [Test]
        public void CheckGettersBuildFromASimpleQarnotUrl()
        {
            var resolve = new DnsResolver("https://api.qarnot.com");
            Assert.AreEqual(resolve.Protocol, "https://");
            Assert.AreEqual(resolve.DomainName, "qarnot.com");
            Assert.AreEqual(resolve.ServiceName, "api");
            Assert.AreEqual(resolve.HostName, "api.qarnot.com");
            Assert.AreEqual(resolve.PathName, string.Empty);
            Assert.AreEqual(resolve.ServiceAsk, "api");
            Assert.AreEqual(resolve.DomainNameAsk, "qarnot.com");
            Assert.AreEqual(resolve.ProtocolAsk, ProtocolType.Tcp);
            Assert.AreEqual(resolve.DnsSrvMatch, true);
        }

        [Test]
        public void CheckGettersBuildFromDifferentQarnotUrl()
        {
            var resolve = new DnsResolver("https://api.qarnot.com");
            Assert.AreEqual(resolve.DomainName, "qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.test.qarnot.com");
            Assert.AreEqual(resolve.DomainName, "test.qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.dev.qarnot.com");
            Assert.AreEqual(resolve.DomainName, "dev.qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.qualif.qarnot.com");
            Assert.AreEqual(resolve.DomainName, "qualif.qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
        }

        [Test]
        public void CheckGettersBuildFromDifferentUrlProtocols()
        {
            var resolve = new DnsResolver("https://api.qarnot.com");
            Assert.AreEqual(resolve.Protocol, "https://");
            resolve = new DnsResolver("ftp://api.qarnot.com");
            Assert.AreEqual(resolve.Protocol, "ftp://");
            resolve = new DnsResolver("mailto:api.qarnot.com");
            Assert.AreEqual(resolve.Protocol, "mailto:");
        }

        [Test]
        public void CheckGettersBuildFromDifferentUrlChangingTheService()
        {
            var resolve = new DnsResolver("https://storage.qarnot.com", serviceAsk: "storage");
            Assert.AreEqual(resolve.DomainName, "qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://storage.test.qarnot.com", serviceAsk: "storage");
            Assert.AreEqual(resolve.DomainName, "test.qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://storage.dev.qarnot.com", serviceAsk: "storage");
            Assert.AreEqual(resolve.DomainName, "dev.qarnot.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
        }

        [Test]
        public void CheckGettersBuildFromDifferentUrlChangingTheDomain()
        {
            var resolve = new DnsResolver("https://api.storage.com", domainNameAsk: "storage.com");
            Assert.AreEqual(resolve.DomainName, "storage.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.test.storage.com", domainNameAsk: "storage.com");
            Assert.AreEqual(resolve.DomainName, "test.storage.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.dev.storage.com", domainNameAsk: "storage.com");
            Assert.AreEqual(resolve.DomainName, "dev.storage.com");
            Assert.IsTrue(resolve.DnsSrvMatch);
        }

        [Test]
        public void CheckGettersBuildFromDifferentUrlChanges()
        {
            var resolve = new DnsResolver("https://storage.qarnot.fr.world.bulot", serviceAsk: "storage", domainNameAsk: "qarnot.fr.world.bulot");
            Assert.IsTrue(resolve.DnsSrvMatch);
        }

        [Test]
        public void CheckGettersBuildFailIfThereIsABadService()
        {
            var resolve = new DnsResolver("https://storage.qarnot.com");
            Assert.IsFalse(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.qarnot.com", serviceAsk: "storage");
            Assert.IsFalse(resolve.DnsSrvMatch);
        }

        [Test]
        public void CheckGettersBuildFailIfThereIsABadDomain()
        {
            var resolve = new DnsResolver("https://api.qarnot.fr");
            Assert.IsFalse(resolve.DnsSrvMatch);
            resolve = new DnsResolver("https://api.qarnot.com", domainNameAsk: "qarnot.fr");
            Assert.IsFalse(resolve.DnsSrvMatch);
        }

        [Test]
        public void VerifyTheBuildUrlReturns()
        {
            var resolve = new DnsResolver("https://api.qarnot.com");
            Assert.AreEqual(resolve.BuildUri("hostname", "test/path"), "https://hostname/test/path");
            resolve = new DnsResolver("ftp://api.qarnot.com");
            Assert.AreEqual(resolve.BuildUri("hostname", null), "ftp://hostname/");
        }

        [Test]
        public void VerifyTheGetPathFromUrlReturns()
        {
            var resolve = new DnsResolver("https://api.qarnot.com");
            Assert.AreEqual(resolve.GetPathFromUrl("https://api.qarnot.com/test/path"), "/test/path");
            Assert.AreEqual(resolve.GetPathFromUrl("https://api.qarnot.com/"), "/");
            Assert.AreEqual(resolve.GetPathFromUrl("https://api.qarnot.com/test/path#?!=blabla://ret.test.qarnot.com"), "/test/path#?!=blabla://ret.test.qarnot.com");
        }
    }
}