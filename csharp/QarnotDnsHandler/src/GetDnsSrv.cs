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
    public class GetDnsSrv : IGetDnsSrv
    {
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
        /// Time, in minutes, of the connection cache before changing it.
        /// </summary>
        /// <value>Time in minutes.</value>
        protected int CacheTimeInMinute { get; }

        /// <summary>
        /// ApiUri given by the user.
        /// </summary>
        /// <value>Api url given.</value>
        protected string ApiUri { get; }

        /// <summary>
        /// Qarnot Tcp url.
        /// </summary>
        /// <value>The tcp uri create withe the qarnot api uri. </value>
        protected string DnsTcpUrl { get; }

        /// <summary>
        /// A valid qarnot DnsSrv find.
        /// </summary>
        /// <value>Is Api uri good and the dns call return a value.</value>
        public bool DnsSrvFind { get; private set; }

        /// <summary>
        /// Safe Lock if the GetDnsSrv is used conncurency.
        /// </summary>
        /// <value>SafeLock for do not multicall the dns.</value>
        protected bool ConcurrencySafeLock { get; set; }

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
        protected ILookupClient DnsSrvClient { get; set; }

        /// <summary>
        /// Addresses find by the dns.
        /// </summary>
        protected class Address
        {
            /// <summary>
            /// quarantain time in seconds.
            /// </summary>
            private const int FailTimeInSeconds = 60 * 5;

            /// <summary>
            /// Last fail time.
            /// </summary>
            protected DateTime FailDate { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Address"/> class.
            /// </summary>
            /// <param name="service">The value to wrap.</param>
            public Address(ServiceHostEntry service)
            {
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
        /// Initializes a new instance of the <see cref="GetDnsSrv"/> class.
        /// Make the DNS query and decide on a backend to use.
        ///     Keep in cache the result of the DNS request and the choice of server.
        ///     use that server for cacheTime minutes
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
        /// <param name="cacheTime">Cache time in minutes.</param>
        /// <param name="rand">Random parmaeter.</param>
        /// <param name="lookupClient">Lookup client used to do the dns call.</param>
        public GetDnsSrv(string uri, int cacheTime = 5, Random rand = null, ILookupClient lookupClient = null)
        {
            Rand = rand ?? new Random();
            DnsSrvClient = lookupClient ?? new LookupClient();
            CacheTimeInMinute = cacheTime;
            ApiUri = uri;
            ConcurrencySafeLock = false;
            DnsList = new List<Address>();
            NextTimeCheck = default(DateTime);
            DnsTcpUrl = GetQarnotApiDnsAddress(uri);
            DnsSrvFind = DnsTcpUrl != null;
        }

        /// <summary>
        /// Get the Uri from the current given address.
        /// </summary>
        /// <param name="uriPath"> The uri following path.</param>
        /// <returns>The uri create with the base uri find and the path given.</returns>
        public Uri GetUri(string uriPath = null)
        {
            var dnsUri = DnsCurrentAddress?.ServiceHostEntry?.HostName;
            var returnUrl = string.IsNullOrEmpty(dnsUri) ? this.ApiUri : "https://" + dnsUri + "/";
            uriPath = uriPath == null ? string.Empty : uriPath;

            return new Uri(returnUrl + uriPath);
        }

        /// <summary>
        /// Put the actual Backend to fail and get an other backend if available
        /// if no backend available, wait 1 minute and start again.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>The Uri of the next dns found address.</returns>
        public async Task<Uri> NextApiUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            DnsCurrentAddress?.Fail();

            int secondsToWaitBeforeRestart = 60;
            DnsCurrentAddress = DnsList.Find(address => address.IsAvailable());
            while (DnsCurrentAddress == null && DnsList.Count > 0)
            {
                await Task.Delay(secondsToWaitBeforeRestart * 1000);
                await CallApiServerUri(cancellationToken);
            }

            return GetUri();
        }

        /// <summary>
        ///  Wait until the SafeLock is release.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task Wait()
        {
            while (ConcurrencySafeLock)
            {
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Update the check time.
        /// </summary>
        protected void UpdateCheckTime()
        {
            NextTimeCheck = DateTime.Now.AddMinutes(CacheTimeInMinute);
        }

        /// <summary>
        /// SafeLock call the Api server Addresses builder.
        /// </summary>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Task.</returns>
        protected async Task CallApiServerUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ConcurrencySafeLock == false)
            {
                ConcurrencySafeLock = true;
                try
                {
                    await BuildDnsSvrListAsync(cancellationToken);
                    UpdateCheckTime();
                }
                catch
                {
                    ConcurrencySafeLock = false;
                    throw;
                }

                ConcurrencySafeLock = false;
            }
            else
            {
                await Wait();
            }
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
        /// <param name="tcpAddress">Address of the dns (format : _api._tcp).</param>
        /// <param name="cancellationToken">cancellation Token.</param>
        /// <returns>Dns Srv response.</returns>
        protected virtual async Task<IEnumerable<ServiceHostEntry>> ResolveDnsSvrUriAsync(string tcpAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dnsList = await DnsSrvClient.ResolveServiceAsync(tcpAddress, "api", System.Net.Sockets.ProtocolType.Tcp);
            DnsSrvFind = dnsList.Length > 0;

            return dnsList;
        }

        /// <summary>
        /// Build a dns list sort by priority and balanced by weight.
        /// </summary>
        /// <param name="addressGiven">Address list to be sort.</param>
        protected void CreateDnsList(IEnumerable<ServiceHostEntry> addressGiven)
        {
            IEnumerable<ServiceHostEntry> sortAddresses = SortByPriorityThemWeight(addressGiven);

            DnsList = sortAddresses.Select(service => new Address(service)).ToList();
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
        /// Get qarnot tcp dns srv api url.
        /// </summary>
        /// <param name="uri">Api Url.</param>
        /// <returns>Qarnot tcp url or null.</returns>
        protected string GetQarnotApiDnsAddress(string uri)
        {
            const string regexStr = @"https://api(\.)(.*\.?)?qarnot\.com";
            var regex = new Regex(regexStr);

            if (regex.Match(uri).Success)
            {
                return Regex.Replace(uri, regexStr, "$2qarnot.com");
            }

            return null;
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
            if (DnsTcpUrl != null)
            {
                var addressGiven = await ResolveDnsSvrUriAsync(DnsTcpUrl, cancellationToken);
                CreateDnsList(addressGiven);
            }
        }
    }
}