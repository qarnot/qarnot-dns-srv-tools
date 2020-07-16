namespace DnsSrvTool
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DnsClient;

#pragma warning disable CA1054, SA1611, CS1591
    // The whole result of a Query
    public class DnsSrvQueryResult
    {
        // public DnsSrvServiceSpecification Service { get; }
        public List<DnsSrvResultEntry> DnsEntries { get; }
        // public DateTime CreationTime { get; }
        public DateTime TtlEndTime { get; }

        // Some other helper fields may provide expiration date, ...
        // DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries, int timeToLive, DnsSrvServiceSpecification service)
        public DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries)
        {
            // Service = service;
            DnsEntries = dnsEntries;
            // CreationTime = DateTime.UtcNow;
            var timeToLive = dnsEntries.Count > 0 ? dnsEntries.Min(entry => entry.TimeToLiveInSec) : 0;
            TtlEndTime = DateTime.UtcNow.AddSeconds(timeToLive);
        }

        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        public bool IsAvailable => TtlEndTime > DateTime.UtcNow;
    }
}