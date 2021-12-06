namespace DnsSrvTool.Test
{
    using System.Net.Http;
    using NUnit.Framework;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307, CA2000, CA1054

    [TestFixture]
    public class TargetQuarantinePolicyServerUnavailableTester
    {
        [Test]
        [Theory]
        public void AllStatusCodeEnums(System.Net.HttpStatusCode statusCodeToTest)
        {
            var targetQuarantine = new TargetQuarantinePolicyServerUnavailable();
            using HttpResponseMessage response = new HttpResponseMessage();
            bool shouldQuarantine = false;
            response.StatusCode = statusCodeToTest;
            if (statusCodeToTest == System.Net.HttpStatusCode.InternalServerError
               || statusCodeToTest == System.Net.HttpStatusCode.BadGateway
               || statusCodeToTest == System.Net.HttpStatusCode.GatewayTimeout
               || statusCodeToTest == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                shouldQuarantine = true;
            }

            Assert.AreEqual(shouldQuarantine, targetQuarantine.ShouldQuarantine(response));
        }
    }
}
