namespace QarnotDnsHandler
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Net.Sockets;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    public class DnsResolver
    {
        public const string DEFAULT_SERVICE = "api";
        public const string DEFAULT_DOMAIN = "qarnot.com";
        public const ProtocolType DEFAULT_PROTOCOL = ProtocolType.Tcp;
        private ILookupClient DnsSrvClient { get; }
        public Uri Url { get; }
        public string Protocol { get { return Url.Scheme + "://"; } }
        public string DomainName { get { return Url.DnsSafeHost; } }
        public string ServiceName { get { return Url.DnsSafeHost.Split('.')[0]; } }
        public string HostName { get { return Url.Host; } }
        public string PathName { get { return Url.AbsolutePath; } }
        public string ServiceAsk { get; }
        public string DomainNameAsk { get; }
        public System.Net.Sockets.ProtocolType ProtocolAsk { get; }
        public bool DnsSrvMatch
        {
            get
            {
                return ServiceName == ServiceAsk && DomainName.EndsWith(DomainNameAsk);
            }
        }

        public DnsResolver(string url, string serviceAsk = DEFAULT_SERVICE, string domainNameAsk = DEFAULT_DOMAIN, System.Net.Sockets.ProtocolType protocolAsk = DEFAULT_PROTOCOL, LookupClient lookupClientstring = null)
        {
            DnsSrvClient = lookupClientstring ?? new LookupClient();
            ServiceAsk = serviceAsk;
            DomainNameAsk = domainNameAsk;
            ProtocolAsk = protocolAsk;
            Url = new Uri(url);
        }

        public string BuildUri(string hostName, string path)
        {
            if (DnsSrvMatch && !string.IsNullOrEmpty(hostName))
            {
                return Protocol + hostName + "/" + path;
            }

            return null;
        }

        public async Task<IEnumerable<ServiceHostEntry>> ResolveServiceAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DnsSrvMatch)
            {
                return await DnsSrvClient.ResolveServiceAsync(Url.DnsSafeHost, ServiceAsk, ProtocolAsk);
            }

            return null;
        }
    }
}