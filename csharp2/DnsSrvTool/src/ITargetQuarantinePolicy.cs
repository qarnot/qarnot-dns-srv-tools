namespace DnsSrvTool
{
    using System;
    using System.Net.Http;

#pragma warning disable CA1054, SA1611, CS1591
    public interface ITargetQuarantinePolicy
    {
        TimeSpan QuarantineDuration { get; }
        bool ShouldQuarantine(HttpResponseMessage response);
    }
}