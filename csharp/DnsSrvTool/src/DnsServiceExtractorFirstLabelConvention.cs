#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;

    /// <summary>
    /// Extractor of object to DnsSrvServiceDescription.
    /// </summary>
    public class DnsServiceExtractorFirstLabelConvention : IDnsServiceExtractor
    {
        /// <summary>
        /// Default protocol.
        /// </summary>
        public const ProtocolType DEFAULTPROTOCOL = ProtocolType.Tcp;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsServiceExtractorFirstLabelConvention"/> class.
        /// </summary>
        /// <param name="protocol">protocol to be used (default, TCP).</param>
        /// <param name="serviceWhiteList">Service White list if you not allow all the services.</param>
        /// <param name="domainWhiteList">Domain White list if you not allow all the domains.</param>
        /// <param name="allowSubDomains">allow the sub-domains of a domain (example test.qarnot.com if there is qarnot.com in the domain white.list).</param>
        public DnsServiceExtractorFirstLabelConvention(ProtocolType? protocol, IEnumerable<string> serviceWhiteList = null, IEnumerable<string> domainWhiteList = null, bool allowSubDomains = false)
        {
            Protocol = protocol ?? DEFAULTPROTOCOL;
            ServiceWhiteList = serviceWhiteList;
            DomainWhiteList = domainWhiteList;
            AllowSubDomains = allowSubDomains;
        }

        /// <summary>
        /// Gets Protocol to use.
        /// </summary>
        /// <value>Getter of a ProtocolType.</value>
        public ProtocolType Protocol { get; }

        /// <summary>
        /// Gets Services allow by the extreactor.
        /// </summary>
        /// <value>Getter of the ServiceWhiteList.</value>
        public IEnumerable<string> ServiceWhiteList { get; }

        /// <summary>
        /// Gets Domains names allow by the extreactor.
        /// </summary>
        /// <value>Getter of the DomainWhiteList.</value>
        public IEnumerable<string> DomainWhiteList { get; }

        /// <summary>
        /// Gets a value indicating whether subdommains:
        /// Allow subdommains to be in the whitelist.
        /// example: DomainWhiteList["qarnot.com"].
        /// subdomaine : ["test.qarnot.com"].
        /// </summary>
        /// <value>Getter of the subdomains.</value>
        private bool AllowSubDomains { get; }

        /// <summary>
        /// Extract a service and a domain from an Uri.
        /// </summary>
        /// <param name="uri">Uri to be extract.</param>
        /// <returns>DnsSrvServiceDescription object.</returns>
        public DnsSrvServiceDescription FromUri(Uri uri)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri), "The uri should not be null.");

            var splitIndex = uri.DnsSafeHost.IndexOf(".");
            var serviceName = uri.DnsSafeHost.Substring(0, splitIndex);
            var domain = uri.DnsSafeHost.Substring(splitIndex + 1);
            if ((ServiceWhiteList != null && !ServiceWhiteList.Contains(serviceName)) ||
                (DomainWhiteList != null &&
                    ((AllowSubDomains && !DomainWhiteList.Any(dom => domain.EndsWith(dom))) ||
                    (!AllowSubDomains && !DomainWhiteList.Contains(domain)))))
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