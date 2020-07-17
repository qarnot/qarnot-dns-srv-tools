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
        public void NullServiceThrowException()
        {
            var selector = new DnsServiceTargetSelectorReal(null, null, 0, 0);
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await selector.SelectHostAsync(null));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task ShouldNotFailWhenQueryServerReturnNoElement()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerierEmpty(), new DnsSrvSortResult(), 10, 10);
            var ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNull(ret);
        }

        [Test]
        public async Task ResetBlacklistForHostShouldNotFail()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, 10);
            var save = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            var ret = save;
            do
            {
                selector.BlacklistHostFor(ret, new TimeSpan(1, 1, 1));
                ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            }
            while (ret != null);
            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNull(ret);
            selector.ResetBlacklistForHost(save);
            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNotNull(ret);
        }

        [Test]
        public async Task ResetShouldNotFail()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, 10);
            var ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            // put everyting in bl
            selector.Reset();
            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNull(ret);
        }
    }
}