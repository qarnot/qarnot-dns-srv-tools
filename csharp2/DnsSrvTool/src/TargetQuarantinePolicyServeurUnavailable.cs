namespace DnsSrvTool
{
    using System;
    using System.Net.Http;

#pragma warning disable CA1054, SA1611, CS1591
    public class TargetQuarantinePolicyServeurUnavailable : ITargetQuarantinePolicy
    {
        public TimeSpan QuarantineDuration { get; }

        public TargetQuarantinePolicyServeurUnavailable(TimeSpan? quarantineDuration = null)
        {
            QuarantineDuration = quarantineDuration ?? new TimeSpan(0, 5, 0);
        }

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