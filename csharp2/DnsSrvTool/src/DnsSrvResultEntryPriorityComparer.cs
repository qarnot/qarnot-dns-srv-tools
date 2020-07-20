namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using DnsClient;

    /// <summary>
    /// IComparer function to sort the DnsSrvResultEntry by priority.
    /// </summary>
    public class DnsSrvResultEntryPriorityComparer : IComparer<DnsSrvResultEntry>
    {
        /// <summary>
        /// Sort the DnsSrvResultEntry by priority.
        /// </summary>
        /// <param name="x">DnsSrvResultEntry 1.</param>
        /// <param name="y">DnsSrvResultEntry 2.</param>
        /// <returns>Upper or lower priority.</returns>
        public int Compare(DnsSrvResultEntry x, DnsSrvResultEntry y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            return x.Priority - y.Priority;
        }
    }

}