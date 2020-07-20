namespace DnsSrvTool
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using DnsClient;

    /// <summary>
    /// DnsSrvQueryResult class.
    /// Used to stock the SRV responses.
    /// </summary>
    public class DnsSrvQueryResult
    {
        /// <summary>
        /// Srv entities given by the Dns.
        /// </summary>
        /// <value>List of DnsSrvResultEntry entities.</value>
        public List<DnsSrvResultEntry> DnsEntries { get; }

        /// <summary>
        /// Ttl of the smallest entity given.
        /// </summary>
        /// <value>DateTime</value>
        public DateTime TtlEndTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvQueryResult"/> class.
        /// </summary>
        /// <param name="dnsEntries">DnsSrvResultEntry list.</param>
        public DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries)
        {
            DnsEntries = dnsEntries;
            int timeToLive = dnsEntries.Count > 0 ? dnsEntries.Min(entry => entry.TimeToLiveInSec) : 1;
            timeToLive = timeToLive > 0 ? timeToLive : 1;
            TtlEndTime = DateTime.UtcNow.AddSeconds(timeToLive);
        }

        /// <summary>
        /// Lower the liveTime to MaxTimeToLiveLeft seconds if the actual lifetime is upper to it.
        /// </summary>
        /// <param name="MaxTimeToLiveLeft">The max life time.</param>
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