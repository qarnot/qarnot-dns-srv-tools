namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;

    public class DnsServiceExtractorFirstLabelConvention : IDnsServiceExtractor
    {
        public const ProtocolType DEFAULT_PROTOCOL = ProtocolType.Tcp;

        public ProtocolType Protocol { get; }

        public IEnumerable<string> ServiceWhiteList { get; }

        public IEnumerable<string> DomainWhiteList { get; }

        private bool AllowSubDomains { get; }

        public DnsServiceExtractorFirstLabelConvention(ProtocolType? protocol, IEnumerable<string> serviceWhiteList = null, IEnumerable<string> domainWhiteList = null, bool allowSubDomains = false)
        {
            Protocol = protocol ?? DEFAULT_PROTOCOL;
            ServiceWhiteList = serviceWhiteList;
            DomainWhiteList = domainWhiteList;
            AllowSubDomains = allowSubDomains;
        }

        public DnsSrvServiceDescription FromUri(Uri uri)
        {
            var splitIndex = uri.DnsSafeHost.IndexOf(".");
            var serviceName = uri.DnsSafeHost.Substring(0, splitIndex);
            var domain = uri.DnsSafeHost.Substring(splitIndex + 1);
            if ((ServiceWhiteList != null && !ServiceWhiteList.Contains(serviceName)) ||
                (DomainWhiteList != null &&
                    ((AllowSubDomains && !DomainWhiteList.Any(dom => domain.EndsWith(dom)) ||
                    (!AllowSubDomains && !DomainWhiteList.Contains(domain))))))
            {
                return null;
            }

            var dnsSrvServiceDescription = new DnsSrvServiceDescription(
                serviceName: serviceName,
                protocol: Protocol,
                domain: domain);

            return dnsSrvServiceDescription;
        }
    }
}