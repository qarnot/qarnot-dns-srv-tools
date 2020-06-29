namespace QarnotDsnHandler.Test
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
    using QarnotDsnHandler;
    // using Moq;
    using Telerik.JustMock;

    [TestFixture]
    public class GetDnsSrvIntegrationTest
    {
        HttpClientHandler Handler;
        FakeLookClient Lookup;
        GetDnsSrvTester DnsTester;

        const string testUrl = "https://api.test.qarnot.com/";

        [SetUp]
        public void SetUp()
        {
            Lookup = new FakeLookClient();

            DnsTester = new GetDnsSrvTester(Lookup);
        }

        [TearDown]
        public void TearDown()
        {
            // HttpHandler?.Dispose();
        }

        public class GetDnsSrvTester : GetDnsSrv
        {
            public IEnumerable<ServiceHostEntry> DnsList = null;

            public int DnsCall = 0;

            public GetDnsSrvTester(ILookupClient lookupClient, string uri = testUrl, int cacheTime = 5, Random rand = null)
            : base(uri, cacheTime, rand, lookupClient)
            {}

            protected override async Task<IEnumerable<ServiceHostEntry>> ResolveDsnSvrUriAsync(string tcpAddress, CancellationToken cancellationToken = default(CancellationToken))
            {
                DnsCall++;
                return DnsList;
            }
        }


        [Test]
        public async Task TestBalanceApiServerUriWithNoListFindReturnTheOriginalUri()
        {
            DnsTester.DnsList = new List<ServiceHostEntry>();
            Uri uri = await DnsTester.BalanceApiServerUri();

            Assert.AreEqual(testUrl, uri.ToString());
        }

        [Test]
        public async Task TestBalanceApiServerUriReturnTheFirstUri()
        {
            DnsTester.DnsList = new List<ServiceHostEntry>()
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
            DnsTester.DnsList = new List<ServiceHostEntry>();
            Uri uri = await DnsTester.BalanceApiServerUri();
            Uri nextUri = await DnsTester.NextApiUri();
            Assert.AreEqual(testUrl, uri.ToString());
        }

        [Test]
        public async Task TestNextApiUriReturnTheNextUri()
        {
            var dnsList = new List<ServiceHostEntry>()
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

            DnsTester.DnsList = dnsList;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsList[0].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[1].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[2].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[3].HostName), uri);
        }

        [Test]
        public async Task TestNextApiUriReturnTheFirstUriAfterAWait()
        {
            var dnsList = new List<ServiceHostEntry>()
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

            DnsTester.DnsList = dnsList;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsList[0].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[1].HostName), uri);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[2].HostName), uri);
            await Task.Delay(60000);
            await DnsTester.NextApiUri();
            uri = DnsTester.GetUri();
            Assert.AreEqual(new Uri("https://" + dnsList[0].HostName), uri);
        }

        [Test]
        public async Task TestNextApiUriWaitAndRetryIfNoMoreUrl()
        {
            var dnsList = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 10,
                    HostName = "address1.qarnot.com",
                },
            };

            DnsTester.DnsList = dnsList;
            var date = DateTime.Now;
            Uri uri = await DnsTester.BalanceApiServerUri();
            Assert.AreEqual(new Uri("https://" + dnsList[0].HostName), uri);
            var lTask = new List<Task>();
            Func<Task> callNext = async () => { await DnsTester.NextApiUri(); };
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            lTask.Add(callNext());
            callNext = async () => {
                await Task.Delay(100);
                dnsList = new List<ServiceHostEntry>()
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
                DnsTester.DnsList = dnsList;
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
        public async Task TestGetUri()
        {
            DnsTester.DnsList = new List<ServiceHostEntry>();
            Uri uri = DnsTester.GetUri();
            Assert.AreEqual(testUrl, uri.ToString());
        }
   }
}