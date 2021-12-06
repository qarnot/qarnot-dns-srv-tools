namespace DnsSrvTool.Test
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307, CA2000, CA1054

    [TestFixture]
    public class DnsServiceTargetSelectorRealTester
    {
        [Test]
        public void NullServiceThrowException()
        {
            var selector = new DnsServiceTargetSelectorReal(null, null, 0, null);
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await selector.SelectHostAsync(null));
            selector = new DnsServiceTargetSelectorReal(null, null, 0, CreateLoggers.CreateILoggerFromNLog());
            ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await selector.SelectHostAsync(null));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task ShouldNotFailWhenQueryServerReturnNoElement()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerierEmpty(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNull(ret);
        }

        [Test]
        public async Task BlacklistForShouldThrowExceptionIfNullValue()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var ret1 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await selector.BlacklistHostForAsync(null, new TimeSpan(1, 1, 1)));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task BlacklistForShouldNotFail()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var ret1 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            await selector.BlacklistHostForAsync(ret1, new TimeSpan(1, 1, 1));
            var ret2 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.AreNotEqual(ret1, ret2);
        }

        [Test]
        public async Task ResetBlacklistForHostShouldNotFail()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var ret1 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            await selector.BlacklistHostForAsync(ret1, new TimeSpan(1, 1, 1));
            var ret2 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            await selector.ResetBlacklistForHostAsync(ret1);
            var ret3 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.AreNotEqual(ret1, ret2);
            Assert.AreEqual(ret1, ret3);
        }

        [Test]
        public async Task ResetBlacklistForHostShouldNotFailNorThrowException()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var ret1 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            await selector.ResetBlacklistForHostAsync(ret1);
            var ret2 = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.AreEqual(ret1, ret2);
        }

        public async Task ResetBlacklistForHostAfterAllHostPutInQuaranine()
        {
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, CreateLoggers.CreateILoggerFromNLog());
            var save = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            var ret = save;
            do
            {
                await selector.BlacklistHostForAsync(ret, new TimeSpan(1, 1, 1));
                ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            }
            while (ret != null);

            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNull(ret);
            await selector.ResetBlacklistForHostAsync(save);
            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNotNull(ret);
        }

        [Test]
        public async Task ResetShouldAllowToRecallTheDns()
        {
            var logger = CreateLoggers.CreateILoggerFromNLog();
            var selector = new DnsServiceTargetSelectorReal(new FakeDnsSrvQuerier(), new DnsSrvSortResult(), 10, logger);
            var ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            do
            {
                await selector.BlacklistHostForAsync(ret, new TimeSpan(1, 1, 1));
                ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            }
            while (ret != null);

            await selector.ResetAsync();
            ret = await selector.SelectHostAsync(new DnsSrvServiceDescription("service", ProtocolType.Tcp, "domain"));
            Assert.IsNotNull(ret);
        }
    }
}
