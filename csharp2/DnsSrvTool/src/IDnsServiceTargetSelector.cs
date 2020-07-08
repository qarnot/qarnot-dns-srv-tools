namespace DnsSrvTool
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    // Deciding which host to use. As an interface for DI, to be able to mock
    // it to test upper layers.
    public interface IDnsServiceTargetSelector
    {
        Task<DnsEndPoint> SelectHost(DnsSrvServiceDescription service);

        // Blacklist a host for some time. No questions asked.
        void BlacklistHostFor(DnsEndPoint host, TimeSpan duration);

        // Immediately remove a host from blacklist
        void ResetBlacklistForHost(DnsEndPoint host);

        // Most implementation (except mocks) will be stateful, a Reset() method will be handy.
        void Reset();
    }
}