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
    public class DnsServiceTargetSelectorRealTester
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        // test semaphore ***************
        // test blacklist something *****
        // test thing is blacklist ******
        // test reset blacklist *********
        // test reset *******************
        // +++++++++++++++++++++++++++ //
        // DnsServiceTargetSelectorReal *
        // SelectHost
        // BlacklistHostFor
        // ResetBlacklistForHost
        // Reset

        [Test]
        public void TestDifferentFromUri()
        {
            var selector = new DnsServiceTargetSelectorReal(null, 0, 0);
        }
    }
}