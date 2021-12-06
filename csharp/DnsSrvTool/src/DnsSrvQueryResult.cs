#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// DnsSrvQueryResult class.
    /// Used to stock the SRV responses.
    /// </summary>
    public class DnsSrvQueryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvQueryResult"/> class.
        /// </summary>
        /// <param name="dnsEntries">DnsSrvResultEntry list.</param>
        public DnsSrvQueryResult(List<DnsSrvResultEntry> dnsEntries)
        {
            if (dnsEntries == null)
            {
                throw new ArgumentNullException(nameof(dnsEntries));
            }

            DnsEntries = dnsEntries;
            int timeToLive = dnsEntries.Count > 0 ? dnsEntries.Min(entry => entry.TimeToLiveInSec) : 1;
            timeToLive = timeToLive > 0 ? timeToLive : 1;
            TtlEndTime = DateTime.UtcNow.AddSeconds(timeToLive);
        }

        /// <summary>
        /// Gets Srv entities given by the Dns.
        /// </summary>
        /// <value>List of DnsSrvResultEntry entities.</value>
        public List<DnsSrvResultEntry> DnsEntries { get; }

        /// <summary>
        /// Gets Ttl of the smallest entity given.
        /// </summary>
        /// <value>End DateTime.</value>
        public DateTime TtlEndTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the result time to live end.
        /// </summary>
        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        /// <summary>
        /// Gets a value indicating whether the result is available.
        /// </summary>
        public bool IsAvailable => TtlEndTime > DateTime.UtcNow;

        /// <summary>
        /// Lower the liveTime to maxTimeToLiveLeft seconds if the actual lifetime is upper to it.
        /// </summary>
        /// <param name="maxTimeToLiveLeft">The max life time.</param>
        public void ReduceLifeTime(uint maxTimeToLiveLeft)
        {
            if (TtlEndTime > DateTime.UtcNow.AddSeconds(maxTimeToLiveLeft))
            {
                TtlEndTime = DateTime.UtcNow.AddSeconds(maxTimeToLiveLeft);
            }
        }
    }
}
