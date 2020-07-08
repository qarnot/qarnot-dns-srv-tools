namespace DnsSrvTool
{
    using System.Net.Http;

    public class TargetQuarantinePolicyServeurUnavailable : ITargetQuarantinePolicy
    {
        public bool ShouldQuarantine(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.GatewayTimeout:
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return true;
                default:
                    return false;
            }
        }
    }
}