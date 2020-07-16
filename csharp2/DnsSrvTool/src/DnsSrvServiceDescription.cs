namespace DnsSrvTool
{
    using System.Net.Sockets;

#pragma warning disable CA1054, SA1611, CS1591
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
    }
}