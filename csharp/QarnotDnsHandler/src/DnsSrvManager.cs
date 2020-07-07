namespace QarnotDnsHandler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

#pragma warning disable SA1623, SA1202, SA1201, CA1056, CA1062, CA1822, CA1054

    /// <summary>
    /// Check and get an srv uri given by the qarnot dns srv address
    /// or return the given address.
    /// </summary>
    public class DnsSrvManager : IDnsSrvManager
    {
        /// <summary>
        /// Default cache time value in seconds
        /// </summary>
        public const int DEFAULT_CACHETIME = 5 * 60;

        /// <summary>
        /// Default fail time before reuse value in seconds
        /// </summary>
        public const int DEFAULT_FAILTIME = 60;

        /// <summary>
        /// default retrieve wait time if no address available
        /// </summary>
        public const int DEFAULT_RETRIEVETIME = 5 * 60;

        private SemaphoreSlim semaphore;

        /// <summary>
        /// Random object create in the constructor.
        /// </summary>
        protected Random Rand { get; }

        /// <summary>
        /// Date when to check the value.
        /// </summary>
        /// <value>DateTime value.</value>
        protected DateTime NextTimeCheck { get; set; }

        /// <summary>
        /// Time connection cache, in seconds, before recall it.
        /// </summary>
        /// <value>Time in seconds.</value>
        protected int CacheTimeInSeconds { get; }

        /// <summary>
        /// Time, in seconds, before reusing an address after a fail.
        /// </summary>
        /// <value>Time in seconds.</value>
        protected int FailTimeInSeconds { get; }

        /// <summary>
        /// Time, in seconds, to wait before recalling the dns if all the dns addresses had fail.
        /// </summary>
        /// <value>Time in seconds.</value>
        protected int RetrieveTimeInSeconds { get; }

        /// <summary>
        /// Dns List find.
        /// </summary>
        private List<Address> DnsList { get; set; }

        /// <summary>
        /// Dns current valid Address used.
        /// </summary>
        protected Address DnsCurrentAddress { get; set; }

        /// <summary>
        /// The dns library.
        /// </summary>
        protected DnsResolver DnsUrlResolver { get; set; }


        /// <summary>
        /// Addresses find by the dns.
        /// </summary>
        protected class Address
        {
            /// <summary>
            /// quarantain time in seconds.
            /// </summary>
            private int FailTimeInSeconds { get; }

            /// <summary>
            /// Last fail time.
            /// </summary>
            protected DateTime FailDate { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Address"/> class.
            /// </summary>
            /// <param name="service">The value to wrap.</param>
            public Address(ServiceHostEntry service, int failTimeInSeconds)
            {
                FailTimeInSeconds = failTimeInSeconds;
                ServiceHostEntry = service;
                FailDate = default(DateTime);
            }

            /// <summary>
            /// service host entry given by the dns srv.
            /// </summary>
            /// <value>The ServiceHostEntry.</value>
            public ServiceHostEntry ServiceHostEntry { get; }

            /// <summary>
            /// Fail.
            /// </summary>
            public void Fail()
            {
                FailDate = DateTime.Now;
            }

            /// <summary>
            /// Is available or is in quarantain.
            /// </summary>
            /// <returns>Is available.</returns>
            public bool IsAvailable()
            {
                return DateTime.Now > FailDate.AddSeconds(FailTimeInSeconds);
            }
        }

        /// <summary>
        /// IComparer function to sort the ServiceHostEntry by priority.
        /// </summary>
        protected class SrvCompare : IComparer<ServiceHostEntry>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvManager"/> class.
        /// Make the DNS query and decide on a backend to use.
        ///     Keep in cache the result of the DNS request and the choice of server.
        ///     use that server for cacheTime
        /// when the cache expires, restart the process
        /// in case a backend becomes unavailable,
        /// put the backend in quarantine for 5 minutes
        /// choose another backend
        /// when the quarantine time expires, the faulty backend will become eligible a new
        /// if you run completely out of available backends,
        /// sleep some time (1 minute),
        /// reset everything and start again.
        /// </summary>
        /// <param name="uri">Uri of the Api.</param>
        /// <param name="cacheTime">Cache time in seconds.</param>
        /// <param name="rand">Random parmaeter.</param>
        /// <param name="lookupClient">Lookup client used to do the dns call.</param>
        public DnsSrvManager(DnsResolver dnsResolver, int cacheTime = DEFAULT_CACHETIME, int failTime = DEFAULT_FAILTIME, int retrieveTime = DEFAULT_RETRIEVETIME, Random rand = null, ILookupClient lookupClient = null)
        {
            semaphore = new SemaphoreSlim(0, 1);
            Rand = rand ?? new Random();
            DnsUrlResolver = dnsResolver;
            CacheTimeInSeconds = cacheTime;
            FailTimeInSeconds = failTime;
            RetrieveTimeInSeconds = retrieveTime;
            // ApiUri = uri;
            DnsList = null;
            NextTimeCheck = default(DateTime);
            // DnsTcpUrl = GetQarnotApiDnsAddress(uri, service_name, protocol, domain);
            // DnsSrvFind = DnsTcpUrl != null;
        }

        /// <summary>
        /// Get the Uri from the current given address.
        /// </summary>
        /// <param name="uriPath"> The uri following path.</param>
        /// <returns>The uri create with the base uri find and the path given.</returns>
        public Uri GetUri(string uriPath = null)
        {
            string urlBase = DnsUrlResolver.buildUri(DnsCurrentAddress?.ServiceHostEntry?.HostName, uriPath);
            return urlBase == null ? null : new Uri(urlBase);
        }

        /// <summary>
        /// Put the actual Backend to fail and get an other backend if available
        /// if no backend available, wait 1 minute and start again.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>The Uri of the next dns found address.</returns>
        public bool NextApiUri()
        {
            DnsCurrentAddress?.Fail();
            DnsCurrentAddress = DnsList.Find(address => address.IsAvailable());
            if (DnsCurrentAddress == null)
            {
                if (NextTimeCheck > DateTime.Now.AddSeconds(RetrieveTimeInSeconds))
                {
                    NextTimeCheck = DateTime.Now.AddSeconds(RetrieveTimeInSeconds);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the check time.
        /// </summary>
        protected void UpdateCheckTime()
        {
            NextTimeCheck = DateTime.Now.AddSeconds(CacheTimeInSeconds);
        }

        /// <summary>
        /// SafeLock call the Api server Addresses builder.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Task.</returns>
        protected async Task CallApiServerUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            semaphore.Release();
            await BuildDnsSvrListAsync(cancellationToken);
            UpdateCheckTime();
        }

        /// <summary>
        /// Return a valid Qarnot Address from the DNS SRV records or return the uri given.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Uri of the string address of the Api.</returns>
        public async Task<Uri> BalanceApiServerUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DateTime.Now > NextTimeCheck)
            {
                await CallApiServerUri(cancellationToken);
            }

            return GetUri();
        }

        /// <summary>
        /// Sort the ServiceHostEntry by priority.
        /// </summary>
        /// <param name="source"> ServiceHostEntries to be sort. </param>
        /// <returns>ServiceHostEntries sorted.</returns>
        protected IEnumerable<ServiceHostEntry> SortByPriority(IEnumerable<ServiceHostEntry> source)
        {
            var result = source.ToArray();
            Array.Sort(result, new SrvCompare());
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
                int splitSize = source.Where((item) => item.Priority == source.First().Priority).Count();

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
        protected IEnumerable<ServiceHostEntry> SortByPriorityThemWeight(IEnumerable<ServiceHostEntry> source)
        {
            source = SortByPriority(source);

            IEnumerable<ServiceHostEntry> nestedSortList = LoadBalanceByWeight(source);

            return nestedSortList;
        }

        /// <summary>
        /// Call the dns and return the dns response.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Dns Srv response.</returns>
        protected virtual async Task<IEnumerable<ServiceHostEntry>> ResolveDnsSvrUriAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await DnsUrlResolver.ResolveServiceAsync(cancellationToken);
            // var dnsList = await DnsSrvClient.ResolveServiceAsync(tcpAddress, "api", System.Net.Sockets.ProtocolType.Tcp);
            // DnsSrvFind = dnsList.Length > 0;

            // return dnsList;
        }

        /// <summary>
        /// Build a dns list sort by priority and balanced by weight.
        /// </summary>
        /// <param name="addressGiven">Address list to be sort.</param>
        protected void CreateDnsList(IEnumerable<ServiceHostEntry> addressGiven)
        {
            if (addressGiven == null)
            {
                addressGiven = new List<ServiceHostEntry>();
            }

            IEnumerable<ServiceHostEntry> sortAddresses = SortByPriorityThemWeight(addressGiven);

            DnsList = sortAddresses.Select(service => new Address(service, FailTimeInSeconds)).ToList();
            if (DnsList.Count > 0)
            {
                DnsCurrentAddress = DnsList.First();
            }
            else
            {
                DnsCurrentAddress = null;
            }
        }

        /// <summary>
        /// Check the address,
        /// replace it to the Srv address if it's match
        /// resolve it, sort it and test the addresses.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Task.</returns>
        protected async Task BuildDnsSvrListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DnsUrlResolver.DnsSrvMatch)
            {
                var addressGiven = await ResolveDnsSvrUriAsync(cancellationToken);
                // var addressGiven = await DnsUrlResolver.ResolveServiceAsync(cancellationToken);
                CreateDnsList(addressGiven);
            }
        }
    }
}
