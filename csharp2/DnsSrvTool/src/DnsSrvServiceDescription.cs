namespace DnsSrvTool
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// DnsSrvServiceDescription class
    /// Stock the required information to do a Dns call.
    /// </summary>
    public class DnsSrvServiceDescription
    {
        /// <summary>
        /// Service name.
        /// </summary>
        /// <value>Service name.</value>
        public string ServiceName { get; }

        /// <summary>
        /// Protocol used.
        /// </summary>
        /// <value>Protocol.</value>
        public ProtocolType Protocol { get; }

        /// <summary>
        /// Domain name.
        /// </summary>
        /// <value>Domain name.</value>
        public string Domain { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvServiceDescription"/> class.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="protocol"></param>
        /// <param name="domain"></param>
        public DnsSrvServiceDescription(string serviceName, ProtocolType protocol, string domain)
        {
            ServiceName = serviceName;
            Protocol = protocol;
            Domain = domain;
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                DnsSrvServiceDescription service = (DnsSrvServiceDescription)obj;
                return (ServiceName == service.ServiceName) && (Protocol == service.Protocol) && (Domain == service.Domain);
            }
        }
    }
}