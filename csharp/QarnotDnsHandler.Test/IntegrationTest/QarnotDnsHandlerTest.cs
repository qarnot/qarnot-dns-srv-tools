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

        private GetDnsSrvTester DnsTester;

        [SetUp]
        public void SetUp()
        {
            Lookup = new LookupClient();

            DnsTester = new GetDnsSrvTester(Lookup);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task TestBalanceApiServerUriWithNoListFindReturnTheOriginalUri()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>();
            Uri uri = await DnsTester.BalanceApiServerUri();

            Assert.AreEqual(TestUrl, uri.ToString());
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
            Uri nextUri = await DnsTester.NextApiUri();
            Assert.AreEqual(TestUrl, uri.ToString());
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
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[1].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[2].HostName), uri);
            await DnsTester.NextApiUri();
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
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[1].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[2].HostName), uri);
            await Task.Delay(60000);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[0].HostName), uri);
        }

        [Test]
        public async Task TestNextApiUriWaitAndRetryIfNoMoreUrl()
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
            };

            DnsTester.DnsTestList = dnsTestList;
            var date = DateTime.Now;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsTestList[0].HostName), uri);
            var lTask = new List<Task>();
            Func<Task> callNext = async () =>
            {
                await DnsTester.NextApiUri();
            };
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            callNext = async () =>
            {
                await Task.Delay(100);
                dnsTestList = new List<ServiceHostEntry>()
                {
                    new ServiceHostEntry()
                    {
                        Port = 430,
                        Priority = 1,
                        Weight = 10,
                        HostName = "address3.qarnot.com",
                    },
                    new ServiceHostEntry()
                    {
                        Port = 430,
                        Priority = 2,
                        Weight = 10,
                        HostName = "address4.qarnot.com",
                    },
                    new ServiceHostEntry()
                    {
                        Port = 430,
                        Priority = 3,
                        Weight = 10,
                        HostName = "address5.qarnot.com",
                    },
                };
                DnsTester.DnsTestList = dnsTestList;
            };
            lTask.Add(callNext());

            await Task.WhenAll(lTask);
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://address3.qarnot.com"), uri);

            Assert.IsTrue(date.AddSeconds(30) < DateTime.Now);
            Assert.IsTrue(date.AddSeconds(90) > DateTime.Now);
            Assert.AreEqual(DnsTester.DnsCall, 2);
        }

        [Test]
        public void TestGetUri()
        {
            DnsTester.DnsTestList = new List<ServiceHostEntry>();
            Uri uri = DnsTester.GetUri();
            Assert.AreEqual(TestUrl, uri.ToString());
        }

        internal class GetDnsSrvTester : GetDnsSrv
        {
            internal GetDnsSrvTester(ILookupClient lookupClient, string baseUrl = TestUrl, int cacheTime = 5, Random rand = null)
            : base(baseUrl, cacheTime, rand, lookupClient)
            {
            }

            internal IEnumerable<ServiceHostEntry> DnsTestList { get; set; } = null;

            internal int DnsCall { get; set; } = 0;

            protected override Task<IEnumerable<ServiceHostEntry>> ResolveDnsSvrUriAsync(string tcpAddress, CancellationToken cancellationToken = default(CancellationToken))
            {
                DnsCall++;
                return Task.FromResult(DnsTestList);
            }
        }
    }
}