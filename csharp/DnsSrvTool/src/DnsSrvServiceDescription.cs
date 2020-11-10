#pragma warning disable CA1303, CA1307

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
        /// Initializes a new instance of the <see cref="DnsSrvServiceDescription"/> class.
        /// </summary>
        /// <param name="serviceName">The dns service.</param>
        /// <param name="protocol">The dns protocol.</param>
        /// <param name="domain">The dns domain.</param>
        public DnsSrvServiceDescription(string serviceName, ProtocolType protocol, string domain)
        {
            ServiceName = serviceName;
            Protocol = protocol;
            Domain = domain;
        }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        /// <value>The service name.</value>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the protocol used.
        /// </summary>
        /// <value>The protocol.</value>
        public ProtocolType Protocol { get; }

        /// <summary>
        /// Gets the Domain name.
        /// </summary>
        /// <value>Domain name.</value>
        public string Domain { get; }

        /// <summary>
        /// Are this object equal to an other DnsSrvServiceDescription.
        /// </summary>
        /// <param name="obj">an other DnsSrvServiceDescription.</param>
        /// <returns>Is equal or not.</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                DnsSrvServiceDescription service = (DnsSrvServiceDescription)obj;
                return GetHashCode() == service.GetHashCode();
            }
        }

        /// <summary>
        /// Get the hash code of the service.
        /// </summary>
        /// <returns>Return int hash code.</returns>
        public override int GetHashCode()
        {
            return ServiceName.GetHashCode() ^ Protocol.GetHashCode() ^ Domain.GetHashCode();
        }
    }
}