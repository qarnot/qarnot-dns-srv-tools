namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDnsServiceTargetSelector
    {
        Task<DnsEndPoint> SelectHostAsync(DnsSrvServiceDescription service);

        void BlacklistHostFor(DnsEndPoint host, TimeSpan duration);

        void ResetBlacklistForHost(DnsEndPoint host);

        void Reset();
    }
}