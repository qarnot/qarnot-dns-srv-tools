namespace QarnotDnsHandler
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    public class DnsResolver
    {
        public const string DEFAULT_SERVICE = "api";
        public const string DEFAULT_DOMAIN = "qarnot.com";
        public const ProtocolType DEFAULT_PROTOCOL = ProtocolType.Tcp;
        private ILookupClient DnsSrvClient { get; }
        public string Protocol { get; }
        public string DomainName { get; }
        public string ServiceName { get; }
        public string HostName { get; }
        public string PathName { get; }
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

            var protocolFind = SplitUrl(url);
            if (protocolFind.Success)
            {
                Protocol = protocolFind.Groups[1].Value;
                HostName = protocolFind.Groups[3].Value;
                ServiceName = protocolFind.Groups[4].Value.Replace(".", "");
                DomainName = protocolFind.Groups[5].Value;
                PathName = protocolFind.Groups[6].Value;
            }
        }

        /// <summary>
        /// Protocol = protocolFind.Groups[1].Value;
        /// HostName = protocolFind.Groups[3].Value;
        /// ServiceName = protocolFind.Groups[4].Value.Replace(".", "");
        /// DomainName = protocolFind.Groups[5].Value;
        /// PathName = protocolFind.Groups[6].Value;
        /// </summary>
        /// <param name="url">the url to split</param>
        /// <returns></returns>
        private Match SplitUrl(string url)
        {
            var uriProtocolRegex = @"^([a-z]*:(//)?)(([a-zA-Z0-9_\-]+\.)([a-zA-Z0-9_\.-]+))(/.*)?";
            return Regex.Match(url, uriProtocolRegex);
        }

        public string GetPathFromUrl(string url)
        {
            var protocolFind = SplitUrl(url);
            if (protocolFind.Success)
            {
                return protocolFind.Groups[6].Value;
            }

            return null;
        }

        public string buildUri(string hostName, string path)
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
                return await DnsSrvClient.ResolveServiceAsync(HostName, ServiceAsk, ProtocolAsk);
            }

            return null;
        }
    }
}