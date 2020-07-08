namespace DnsSrvTool
{
    using System;
    using System.Net.Sockets;

    public class DnsServiceExtractorFirstLabelConvention : IDnsServiceExtractor
    {
        public const ProtocolType DEFAULT_PROTOCOL = ProtocolType.Tcp;
        public ProtocolType Protocol { get; }

        public DnsServiceExtractorFirstLabelConvention(ProtocolType? protocol)
        {
            Protocol = protocol ?? DEFAULT_PROTOCOL;
        }

        public DnsSrvServiceDescription FromUri(Uri uri)
        {
            var splitIndex = uri.DnsSafeHost.IndexOf(".");
            var serviceName = uri.DnsSafeHost.Substring(0, splitIndex);
            var domain = uri.DnsSafeHost.Substring(splitIndex + 1);

            var dnsSrvServiceDescription = new DnsSrvServiceDescription(
                serviceName:serviceName,
                protocol:Protocol,
                domain:domain);

            return dnsSrvServiceDescription;
        }
    }
}