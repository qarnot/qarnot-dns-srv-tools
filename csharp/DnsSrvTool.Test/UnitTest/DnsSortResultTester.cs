namespace DnsSrvTool.Test
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

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307, CA2000, CA1054

    [TestFixture]
    public class DnsSortResultTester : DnsSrvSortResult
    {
        public DnsSortResultTester()
        {
        }

        [Test]
        public void TestEntityFullString()
        {
            var entry = new DnsSrvResultEntry("hostname", 430, 4, 10, 42);
            Assert.IsTrue(entry.ToString("f").Contains("HostName: hostname Port: 430 Priority: 4 Weight: 10 TimeToLiveInSec: 42 CreationTime: "));
        }

        [Test]
        public void TestResultIsAlive()
        {
            var result = new DnsSrvQueryResult(new List<DnsSrvResultEntry>());
            var result2 = new DnsSrvQueryResult(new List<DnsSrvResultEntry>() { new DnsSrvResultEntry("hostname", 430, 4, 10, 42) });
            Assert.IsTrue(result.IsAlive);
            Assert.IsTrue(result2.IsAlive);
        }

        [Test]
        public void CheckSortByPriority()
        {
            var source = new List<DnsSrvResultEntry>()
            {
                new DnsSrvResultEntry("hostname", 430, 4, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 6, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 1, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 3, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 2, 10, 42),
            };
            var result = SortByPriority(source).ToList();
            Assert.AreEqual(1, result[0].Priority);
            Assert.AreEqual(2, result[1].Priority);
            Assert.AreEqual(3, result[2].Priority);
            Assert.AreEqual(4, result[3].Priority);
            Assert.AreEqual(6, result[4].Priority);
        }

        [Test]
        public void CheckTheLoadBalanceByWeight()
        {
            var source = new List<DnsSrvResultEntry>()
            {
                new DnsSrvResultEntry("hostname", 430, 1, 1, 42),
                new DnsSrvResultEntry("hostname", 430, 1, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 1, 50, 42),
                new DnsSrvResultEntry("hostname", 430, 1, 20, 42),
            };
            var numTest = 300;
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
            var source = new List<DnsSrvResultEntry>()
            {
                new DnsSrvResultEntry("hostname", 430, 1, 100, 42),
                new DnsSrvResultEntry("hostname", 430, 20, 100, 42),
                new DnsSrvResultEntry("hostname", 430, 2, 1, 42),
                new DnsSrvResultEntry("hostname", 430, 2, 10, 42),
                new DnsSrvResultEntry("hostname", 430, 2, 50, 42),
                new DnsSrvResultEntry("hostname", 430, 2, 20, 42),
            };
            var numTest = 300;
            var count = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 81 * numTest; i++)
            {
                var result = BalanceSortServiceList(source).ToList();

                // test the highest priority
                Assert.AreEqual(1, result[0].Priority);

                // test the lowest priority
                Assert.AreEqual(20, result[5].Priority);

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