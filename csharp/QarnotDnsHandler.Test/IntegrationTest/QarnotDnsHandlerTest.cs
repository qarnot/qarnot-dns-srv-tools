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
    public class QarnotDnsHandlerTest
    {
        private const string TestUrl = "https://api.test.qarnot.com/";

        private ILookupClient Lookup;

        private DnsSrvManagerTester DnsTester;

        [SetUp]
        public void SetUp()
        {
            Lookup = new LookupClient();

            DnsTester = new DnsSrvManagerTester(Lookup);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task TestBalanceApiServerUriWithNoListFindReturnNull()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>();
            Uri uri = await DnsTester.BalanceApiServerUri();

            Assert.AreEqual(null, uri);
        }

        [Test]
        public async Task TestBalanceApiServerUriReturnTheFirstUri()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 4,
                    Weight = 10,
                    HostName = "address1.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 6,
                    Weight = 10,
                    HostName = "address2.qarnot.com",
                },
            };
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://address1.qarnot.com"), uri);
        }

        [Test]
        public async Task TestNextApiUri()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>();
            Uri uri = await DnsTester.BalanceApiServerUri();
            DnsTester.NextApiUri();
            Uri nextUri = DnsTester.GetUri();
            Assert.AreEqual(null, uri);
        }

        [Test]
        public async Task TestNextApiUriReturnTheNextUri()
        {
            var dnsTestList = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 10,
                    HostName = "address1.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 10,
                    HostName = "address2.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 3,
                    Weight = 10,
                    HostName = "address3.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 4,
                    Weight = 10,
                    HostName = "address4.qarnot.com",
                },
            };

            DnsTester.DnsTestList = dnsTestList;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[0].HostName), uri);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[1].HostName), uri);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[2].HostName), uri);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[3].HostName), uri);
        }

        [Test]
        public async Task TestNextApiUriReturnTheFirstUriAfterAWait()
        {
            var dnsTestList = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 10,
                    HostName = "address1.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 10,
                    HostName = "address2.qarnot.com",
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 3,
                    Weight = 10,
                    HostName = "address3.qarnot.com",
                },
            };

            DnsTester.DnsTestList = dnsTestList;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[0].HostName), uri);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[1].HostName), uri);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[2].HostName), uri);
            await Task.Delay(11000);
            DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[0].HostName), uri);
        }

        [Test]
        public void TestGetUri()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>();
            Uri uri = DnsTester.GetUri();
            Assert.AreEqual(null, uri);
        }

        internal class DnsSrvManagerTester : DnsSrvManager
        {
            internal DnsSrvManagerTester(ILookupClient lookupClient, string baseUrl = TestUrl, int cacheTime = 10, Random rand = null)
            : base(new DnsResolver(baseUrl), cacheTime, 10, 10, rand, lookupClient)
            {
            }

            internal IEnumerable<ServiceHostEntry> DnsTestList { get; set; } = null;

            internal int DnsCall { get; set; } = 0;

            protected override Task<IEnumerable<ServiceHostEntry>> ResolveDnsSvrUriAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                DnsCall++;
                return Task.FromResult(DnsTestList);
            }
        }
    }
}