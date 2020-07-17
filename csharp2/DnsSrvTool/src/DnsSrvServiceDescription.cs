namespace DnsSrvTool
{
    using System;
    using System.Net.Sockets;

    public class DnsSrvServiceDescription
    {
        public string ServiceName { get; }
        public ProtocolType Protocol { get; }
        public string Domain { get; }

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