namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    public class DnsSrvQuerier: IDnsSrvQuerier
    {
        private ILookupClient LookupClient { get; }
        private Random Rand { get; }

        public DnsSrvQuerier(ILookupClient lookupClient)
        {
            LookupClient = lookupClient;
            Rand = new Random();
        }

        /// <summary>
        /// Sort the ServiceHostEntry by priority.
        /// </summary>
        /// <param name="source"> ServiceHostEntries to be sort. </param>
        /// <returns>ServiceHostEntries sorted.</returns>
        protected IEnumerable<ServiceHostEntry> SortByPriority(IEnumerable<ServiceHostEntry> source)
        {
            var result = source.ToArray();
            Array.Sort(result, new ServiceHostEntryPriorityComparer());
            return result;
        }

        /// <summary>
        /// Return a sorted list using the weight to have a random distribution with the weight probability.
        /// </summary>
        /// <param name="priorityEnumerable"> ServiceHostEntries priority list.</param>
        /// <returns> ServiceHostEntries sort using the weight to draw the list.</returns>
        protected IEnumerable<ServiceHostEntry> LoadBalancePriorityArrayByWeight(IEnumerable<ServiceHostEntry> priorityEnumerable)
        {
            // sort it with random extract each element with it's weight
            var drawByWeight = new List<ServiceHostEntry>();

            var list = priorityEnumerable.ToList();

            while (list.Any())
            {
                int weightSum = list.Sum((x) => x.Weight);
                int weightIncrement = 0;
                var random = Rand.Next(0, weightSum - 1);
                var priorityGet = list.First((x) =>
                    {
                        weightIncrement += x.Weight;
                        return weightIncrement > random;
                    });
                drawByWeight.Add(priorityGet);
                list.Remove(priorityGet);
            }

            return drawByWeight;
        }

        /// <summary>
        /// Split the list by priority and draw it using the weight to sort it.
        /// </summary>
        /// <param name="source">ServiceHostEntries sort by priority.</param>
        /// <returns>ServiceHostEntries sort by priority with the same priorities mix to have a weighted draw.</returns>
        protected IEnumerable<ServiceHostEntry> LoadBalanceByWeight(IEnumerable<ServiceHostEntry> source)
        {
            var weightSorted = new List<ServiceHostEntry>();
            while (source.Any())
            {
                // get the tab size of the highest priority
                int splitSize = source.Where(item => item.Priority == source.First().Priority).Count();

                // extract it
                var priorityEnumerable = source.Take(splitSize);

                // remove it from the source list
                source = source.Skip(splitSize);

                weightSorted = weightSorted.Concat(LoadBalancePriorityArrayByWeight(priorityEnumerable)).ToList();
            }

            return weightSorted;
        }

        /// <summary>
        /// Return a list sort by priority them randomely using the by weight.
        /// </summary>
        /// <param name="source">ServiceHostEntries.</param>
        /// <returns>Sorted ServiceHostEntries.</returns>
        protected IEnumerable<ServiceHostEntry> BalanceSortServiceList(IEnumerable<ServiceHostEntry> serviceList)
        {
            serviceList = SortByPriority(serviceList);
            return LoadBalanceByWeight(serviceList);
        }

        public async Task<DnsSrvQueryResult> QueryService(DnsSrvServiceDescription service, int resultLifeTime)
        {
            int FailTimeInSeconds = 5;
            var dnsServicesHost = await LookupClient.ResolveServiceAsync(service.Domain, service.ServiceName, service.Protocol);
            dnsServicesHost = BalanceSortServiceList(dnsServicesHost).ToArray();
            var queryResult = dnsServicesHost.Select(serviceHost => new DnsSrvResultEntry(serviceHost.HostName, serviceHost.Port, serviceHost.Priority, serviceHost.Weight, resultLifeTime)).ToList();
            return new DnsSrvQueryResult(queryResult, resultLifeTime);
        }
    }
}