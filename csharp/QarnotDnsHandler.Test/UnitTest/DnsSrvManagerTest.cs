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

#pragma warning disable CA1305, CA1303

    [TestFixture]
    public class DnsSrvManagerTest : DnsSrvManager
    {
        private static ILookupClient lookClient = new LookupClient();
        public DnsSrvManagerTest()
                : base(new DnsResolver("https://api.qarnot.com"), lookupClient: lookClient)
        {
        }

        [Test]
        public void CheckSortByPriority()
        {
            var source = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 4,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 6,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 3,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 10,
                },
            };
            var result = SortByPriority(source).ToList();
            Assert.AreEqual(1, result[0].Priority);
            Assert.AreEqual(2, result[1].Priority);
            Assert.AreEqual(3, result[2].Priority);
            Assert.AreEqual(4, result[3].Priority);
            Assert.AreEqual(6, result[4].Priority);
        }

        [Test]
        public void VerifyGetUriReturn()
        {
            DnsCurrentAddress = null;
            Assert.AreEqual(GetUri(), null);
            DnsCurrentAddress = new Address(null, 1);
            Assert.AreEqual(GetUri(), null);
            DnsCurrentAddress = new Address(
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 4,
                    Weight = 10,
                    HostName = "testSuccess",
                },
                1);
            Assert.AreEqual(GetUri(), "https://testSuccess");
        }

        [Test]
        public void CheckTheLoadBalanceByWeight()
        {
            var source = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 1,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 50,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 20,
                },
            };
            var numTest = 100;
            var count = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 81 * numTest; i++)
            {
                var result = LoadBalancePriorityArrayByWeight(source).ToList();
                if (result[0].Weight == 1)
                {
                    count[0] += 1;
                }
                else if (result[0].Weight == 10)
                {
                    count[1] += 1;
                }
                else if (result[0].Weight == 50)
                {
                    count[2] += 1;
                }
                else if (result[0].Weight == 20)
                {
                    count[3] += 1;
                }
            }

            Assert.That(count[0], Is.EqualTo(1 * numTest).Within(30).Percent);
            Assert.That(count[1], Is.EqualTo(10 * numTest).Within(30).Percent);
            Assert.That(count[2], Is.EqualTo(50 * numTest).Within(30).Percent);
            Assert.That(count[3], Is.EqualTo(20 * numTest).Within(30).Percent);

            foreach (var item in count)
            {
                Console.WriteLine("count :" + item.ToString());
            }
        }

        [Test]
        public void CheckSortByPriorityThemWeight()
        {
            var source = new List<ServiceHostEntry>()
            {
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 1,
                    Weight = 100,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 20,
                    Weight = 100,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 1,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 10,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 50,
                },
                new ServiceHostEntry()
                {
                    Port = 430,
                    Priority = 2,
                    Weight = 20,
                },
            };
            var numTest = 100;
            var count = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 81 * numTest; i++)
            {
                var result = SortByPriorityThemWeight(source).ToList();
                Assert.AreEqual(1, result[0].Priority); // test the priority high
                Assert.AreEqual(20, result[5].Priority); // test the priority low
                if (result[1].Weight == 1)
                {
                    count[0] += 1;
                }
                else if (result[1].Weight == 10)
                {
                    count[1] += 1;
                }
                else if (result[1].Weight == 50)
                {
                    count[2] += 1;
                }
                else if (result[1].Weight == 20)
                {
                    count[3] += 1;
                }
            }

            // Test the load balancer
            Assert.That(count[0], Is.EqualTo(1 * numTest).Within(30).Percent);
            Assert.That(count[1], Is.EqualTo(10 * numTest).Within(30).Percent);
            Assert.That(count[2], Is.EqualTo(50 * numTest).Within(30).Percent);
            Assert.That(count[3], Is.EqualTo(20 * numTest).Within(30).Percent);

            foreach (var item in count)
            {
                Console.WriteLine("count :" + item.ToString());
            }
        }
    }
}