namespace DnsSrvTool
{
    using System;
    using System.Net.Http;

    public interface ITargetQuarantinePolicy
    {
        TimeSpan QuarantineDuration { get; }
        bool ShouldQuarantine(HttpResponseMessage response);
    }
}