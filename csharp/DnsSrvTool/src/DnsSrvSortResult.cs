#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// DnsSrvSortResult class.
    /// </summary>
    public class DnsSrvSortResult : IDnsSrvSortResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvSortResult"/> class.
        /// </summary>
        /// <param name="randomSeed">Random seed if determinist random is needed.</param>
        public DnsSrvSortResult(int? randomSeed = null)
        {
            Rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        }

        private Random Rand { get; }

        /// <summary>
        /// Sort a DnsSrvQueryResult.
        /// </summary>
        /// <param name="result">Result to be sort.</param>
        /// <returns>return the result object sorted.</returns>
        public DnsSrvQueryResult Sort(DnsSrvQueryResult result)
        {
            if (result?.DnsEntries == null || result.DnsEntries.Count == 0)
            {
                return result;
            }

            var sortEntries = BalanceSortServiceList(result.DnsEntries);
            result.DnsEntries.Clear();
            result.DnsEntries.AddRange(sortEntries);
            return result;
        }

        /// <summary>
        /// Sort the DnsSrvResultEntry by priority.
        /// </summary>
        /// <param name="source"> ServiceHostEntries to be sort. </param>
        /// <returns>ServiceHostEntries sorted.</returns>
        protected static IEnumerable<DnsSrvResultEntry> SortByPriority(IEnumerable<DnsSrvResultEntry> source)
        {
            var result = source.ToArray();
            Array.Sort(result, new DnsSrvResultEntryPriorityComparer());
            return result;
        }

        /// <summary>
        /// Return a sorted list using the weight to have a random distribution with the weight probability.
        /// </summary>
        /// <param name="priorityEnumerable"> ServiceHostEntries priority list.</param>
        /// <returns> ServiceHostEntries sort using the weight to draw the list.</returns>
        protected IEnumerable<DnsSrvResultEntry> LoadBalancePriorityArrayByWeight(IEnumerable<DnsSrvResultEntry> priorityEnumerable)
        {
            var drawByWeight = new List<DnsSrvResultEntry>();

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
        protected IEnumerable<DnsSrvResultEntry> LoadBalanceByWeight(IEnumerable<DnsSrvResultEntry> source)
        {
            var weightSorted = new List<DnsSrvResultEntry>();
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
        /// <param name="serviceList">DnsSrv Result Entities.</param>
        /// <returns>Sorted ServiceHostEntries.</returns>
        protected IEnumerable<DnsSrvResultEntry> BalanceSortServiceList(IEnumerable<DnsSrvResultEntry> serviceList)
        {
            serviceList = SortByPriority(serviceList);
            return LoadBalanceByWeight(serviceList);
        }
    }
}
