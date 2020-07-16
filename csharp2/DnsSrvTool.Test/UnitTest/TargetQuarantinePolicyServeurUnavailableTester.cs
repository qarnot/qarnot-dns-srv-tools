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
    public class TargetQuarantinePolicyServeurUnavailableTester
    {
        [Test]
        [Theory]
        public void AllStatusCodeEnums(System.Net.HttpStatusCode statusCodeToTest)
        {
            var targetQuarantine = new TargetQuarantinePolicyServeurUnavailable();
            using HttpResponseMessage response = new HttpResponseMessage();
            bool shouldQuarantaine = false;
            response.StatusCode = statusCodeToTest;
            if (statusCodeToTest == System.Net.HttpStatusCode.InternalServerError
               || statusCodeToTest == System.Net.HttpStatusCode.BadGateway
               || statusCodeToTest == System.Net.HttpStatusCode.GatewayTimeout
               || statusCodeToTest == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                shouldQuarantaine = true;
            }

            Assert.AreEqual(shouldQuarantaine, targetQuarantine.ShouldQuarantine(response));
        }
    }
}