namespace DnsSrvTool.Test
{
    using System.Collections.Generic;
    using System.Net.Sockets;
    using DnsClient;
    using DnsClient.Protocol;
    using Moq;
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303, CA1304, CA2000, CA1054

    [TestFixture]
    public class DnsSrvQuerierTester : DnsSrvQuerier
    {
        public DnsSrvQuerierTester()
            : base(null)
        {
        }

        [Test]
        public void ResolveServiceProcessResultReturnAEntityList()
        {
            IDnsQueryResponse result = null;
            var mockResult = new Mock<IDnsQueryResponse>();
            DnsString canonicalName = DnsClient.DnsString.Parse("hostname.com");
            ResourceRecordInfo info = new ResourceRecordInfo(canonicalName, DnsClient.Protocol.ResourceRecordType.SRV, DnsClient.QueryClass.IN, 10, 1);

            var record = new SrvRecord(info, 1, 10, 443, canonicalName);
            var cNameRecord = new CNameRecord(info, canonicalName);

            List<SrvRecord> answers = new List<SrvRecord>() { record };
            mockResult.Setup(foo => foo.Answers).Returns(answers);
            mockResult.Setup(foo => foo.Additionals).Returns(new List<CNameRecord>() { cNameRecord });
            result = mockResult.Object;
            List<DnsSrvResultEntry> ret = ResolveServiceProcessResult(result);
        }

        [Test]
        public void CreateDnsQueryStringReturnTheGoodString()
        {
            DnsSrvServiceDescription service = new DnsSrvServiceDescription("test", ProtocolType.Tcp, "qarnot.com");
            string queryString = CreateDnsQueryString(service);
            Assert.AreEqual("_test._tcp.qarnot.com.", queryString.ToLower());

            service = new DnsSrvServiceDescription("test2", ProtocolType.Ipx, "hello.qarnot.com");
            queryString = CreateDnsQueryString(service);
            Assert.AreEqual("_test2._ipx.hello.qarnot.com.", queryString.ToLower());
        }
    }
}
