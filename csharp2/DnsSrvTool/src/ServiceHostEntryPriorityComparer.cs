namespace DnsSrvTool
{
    using System.Collections.Generic;
    using DnsClient;

    /// <summary>
    /// IComparer function to sort the ServiceHostEntry by priority.
    /// </summary>
    public class ServiceHostEntryPriorityComparer : IComparer<ServiceHostEntry>
    {
        /// <summary>
        /// Sort the ServiceHostEntry by priority.
        /// </summary>
        /// <param name="x">ServiceHostEntry 1.</param>
        /// <param name="y">ServiceHostEntry 2.</param>
        /// <returns>Upper or lower priority.</returns>
        public int Compare(ServiceHostEntry x, ServiceHostEntry y)
        {
            return x.Priority - y.Priority;
        }
    }

}