namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using DnsClient;

    // The whole result of a Query
    public class DnsSrvQueryResult
    {
        // public DnsSrvServiceSpecification Service { get; }
        public int TimeToLive { get; set; }
        public List<DnsSrvResultEntry> DnsEntries { get; }
        public DateTime CreationTime { get; }
        public DateTime TtlEndTime { get; }

        // Some other helper fields may provide expiration date, ...
        // DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries, int timeToLive, DnsSrvServiceSpecification service)
        public DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries, int timeToLive)
        {
            // Service = service;
            TimeToLive = timeToLive;
            DnsEntries = dnsEntries;
            CreationTime = DateTime.Now;
            TtlEndTime = DateTime.Now.AddSeconds(timeToLive);
        }

        public bool IsAlive()
        {
            return TtlEndTime > DateTime.Now;
        }

        public bool IsAvailable()
        {
            return TtlEndTime > DateTime.Now;
        }
    }
}