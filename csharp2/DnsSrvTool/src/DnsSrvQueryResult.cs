namespace DnsSrvTool
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DnsClient;

    public class DnsSrvQueryResult
    {
        public List<DnsSrvResultEntry> DnsEntries { get; }

        public DateTime TtlEndTime { get; private set; }

        public DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries)
        {
            DnsEntries = dnsEntries;
            int timeToLive = dnsEntries.Count > 0 ? dnsEntries.Min(entry => entry.TimeToLiveInSec) : 1;
            timeToLive = timeToLive > 0 ? timeToLive : 1;
            TtlEndTime = DateTime.UtcNow.AddSeconds(timeToLive);
        }

        public void ReduceLiveTime(uint MaxTimeToLiveLeft)
        {
            if (TtlEndTime > DateTime.UtcNow.AddSeconds(MaxTimeToLiveLeft))
            {
                TtlEndTime = DateTime.UtcNow.AddSeconds(MaxTimeToLiveLeft);
            }
        }

        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        public bool IsAvailable => TtlEndTime > DateTime.UtcNow;
    }
}