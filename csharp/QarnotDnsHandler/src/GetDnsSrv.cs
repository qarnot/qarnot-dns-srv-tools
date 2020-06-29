namespace QarnotDsnHandler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    /// <summary>
    /// check and get an srv uri given by the quarnot dns srv address
    /// or return the given address
    /// </summary>
    public class GetDnsSrv: IGetUri
    {
        /// <summary>
        /// Random object create in the constructor
        /// </summary>
        protected Random Rand { get; }

        /// <summary>
        /// Date when to check the value
        /// </summary>
        /// <value>DateTime value</value>
        protected DateTime NextTimeCheck { get; set; }

        /// <summary>
        /// Time, in minutes, of the connection cache before changing it.
        /// </summary>
        /// <value>Time in minutes</value>
        protected int CacheTimeInMinute { get; }

        /// <summary>
        /// ApiUri given by the user.
        /// </summary>
        /// <value>Api url given.</value>
        protected string ApiUri { get; }

        /// <summary>
        /// Qarnot Tcp url
        /// </summary>
        /// <value></value>
        protected string DnsTcpUrl { get; }

        /// <summary>
        /// A valid qarnot DnsSrv find
        /// </summary>
        /// <value></value>
        public bool DnsSrvFind { get; private set; }

        /// <summary>
        /// Safe Lock if the GetDnsSrv is used conncurency.
        /// </summary>
        /// <value></value>
        protected bool ConcurrencySafeLock { get; set; }

        /// <summary>
        /// Dsn List find
        /// </summary>
        protected List<Address> DsnList;

        /// <summary>
        /// Dns current valid Address used
        /// </summary>
        protected Address DsnCurrentAddress;

        /// <summary>
        /// The dns library
        /// </summary>
        /// <returns></returns>
        protected ILookupClient DnsSrvClient;

        /// <summary>
        /// Addresses find by the dns
        /// </summary>
        protected class Address
        {
            /// <summary>
            /// quarantain time in seconds
            /// </summary>
            private const int failTimeInSeconds = 60 * 5;

            /// <summary>
            /// Last fail time
            /// </summary>
            protected DateTime failDate;

            public Address(ServiceHostEntry service)
            {
                serviceHostEntry = service;
                failDate = default(DateTime);
            }

            /// <summary>
            /// service host entry given by the dns srv
            /// </summary>
            /// <value></value>
            public ServiceHostEntry serviceHostEntry { get; }

            /// <summary>
            /// Fail
            /// </summary>
            public void Fail()
            {
                failDate = DateTime.Now;
            }

            /// <summary>
            /// Is available or is in quarantain
            /// </summary>
            /// <returns>is available</returns>
            public bool IsAvailable()
            {
                return DateTime.Now > failDate.AddSeconds(failTimeInSeconds);
            }
        }

        /// <summary>
        /// IComparer function to sort the ServiceHostEntry by priority
        /// </summary>
        protected class SrvCompare : IComparer<ServiceHostEntry>
        {
            /// <summary>
            /// sort the ServiceHostEntry by priority
            /// </summary>
            /// <param name="x">ServiceHostEntry 1</param>
            /// <param name="y">ServiceHostEntry 2</param>
            /// <returns>Upper or lower priority</returns>
            public int Compare(ServiceHostEntry x, ServiceHostEntry y)
            {
                return x.Priority - y.Priority;
            }
        }

        /// <summary>
        /// make the DNS query and decide on a backend to use.
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
        public GetDnsSrv(string uri, int cacheTime = 5, Random rand = null, ILookupClient lookupClient = null)
        {
            Rand = rand ?? new Random();
            DnsSrvClient = lookupClient ?? new LookupClient();
            CacheTimeInMinute = cacheTime;
            ApiUri = uri;
            ConcurrencySafeLock = false;
            DsnList = new List<Address>();
            NextTimeCheck = default(DateTime);
            DnsTcpUrl = GetQarnotApiDnsAddress(uri);
            DnsSrvFind = DnsTcpUrl != null;
        }

        /// <summary>
        /// Get the Uri from the current given address
        /// </summary>
        /// <returns></returns>
        public Uri GetUri(string uriCall = null)
        {
            var dnsUri = DsnCurrentAddress?.serviceHostEntry?.HostName;
            var returnUrl = string.IsNullOrEmpty(dnsUri) ? this.ApiUri : "https://" + dnsUri + "/";
            uriCall = uriCall == null ? string.Empty : uriCall;

            return new Uri(returnUrl + uriCall);
        }

        /// <summary>
        /// Put the actual Backend to fail and get an other backend if available
        /// if no backend available, wait 1 minute and start again.
        /// </summary>
        /// <returns> </returns>
        public async Task<Uri> NextApiUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            DsnCurrentAddress?.Fail();

            int secondsToWaitBeforeRestart = 60;
            // get dns valid address
            DsnCurrentAddress = DsnList.Find(address => address.IsAvailable()); // need to check with a get?
            while (DsnCurrentAddress == null && DsnList.Count > 0)
            {
                await Task.Delay(secondsToWaitBeforeRestart * 1000);
                await CallApiServerUri(cancellationToken);
            }

            return GetUri();
        }

        /// <summary>
        ///  Wait until the SafeLock is release.
        /// </summary>
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
        /// SafeLock call the Api server Addresses builder
        /// </summary>
        protected async Task CallApiServerUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ConcurrencySafeLock == false)
            {
                ConcurrencySafeLock = true;
                try
                {
                    await BuildDsnSvrListAsync(cancellationToken);
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
        /// Return a valid Qarnot Address from the DNS SRV records or return the uri given
        /// </summary>
        /// <returns>Uri of the string address of the Api</returns>
        public async Task<Uri> BalanceApiServerUri(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DateTime.Now > NextTimeCheck)
            {
                await CallApiServerUri(cancellationToken);
            }

            return GetUri();
        }

        /// <summary>
        /// Sort the ServiceHostEntry by priority
        /// </summary>
        /// <param name="source"> ServiceHostEntries to be sort </param>
        /// <returns>ServiceHostEntries sorted</returns>
        protected IEnumerable<ServiceHostEntry> SortByPriority(IEnumerable<ServiceHostEntry> source)
        {
            var result = source.ToArray();
            Array.Sort(result, new SrvCompare());
            return result;
        }

        /// <summary>
        /// return a sorted list using the weight to have a random distribution with the weight probability
        /// </summary>
        /// <param name="priorityEnumerable"> ServiceHostEntries priority list </param>
        /// <returns> ServiceHostEntries sort using the weight to draw the list</returns>
        protected IEnumerable<ServiceHostEntry> LoadBalancePriorityArrayByWeight(IEnumerable<ServiceHostEntry> priorityEnumerable)
        // Unit test need
        {
            // sort it with random extract each element with it's weight
            var drawByWeight = new List<ServiceHostEntry>();

            var list = priorityEnumerable.ToList();

            while (list.Any())
            {
                int weightSum = list.Sum((x) => x.Weight);
                int weightIncrement = 0;
                var random = Rand.Next(0, weightSum - 1);
                var priorityGet = list.First((x) => {
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
        /// <param name="source">ServiceHostEntries sort by priority</param>
        /// <returns>ServiceHostEntries sort by priority with the same priorities mix to have a weighted draw</returns>
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
        /// return a list sort by priority them randomely using the by weight
        /// </summary>
        /// <param name="source">ServiceHostEntries</param>
        /// <returns>Sorted ServiceHostEntries</returns>
        protected IEnumerable<ServiceHostEntry> SortByPriorityThemWeight(IEnumerable<ServiceHostEntry> source)
        {
            source = SortByPriority(source);

            IEnumerable<ServiceHostEntry> nestedSortList = LoadBalanceByWeight(source);

            return nestedSortList;
        }

        /// <summary>
        /// Do a get request to the api server to check it's validity;
        /// </summary>
        /// <returns> return an HttpResponseMessage</returns>
        // protected async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        // {
        //     _client.BaseAddress = new Uri(url);
        //     var result = await _client.GetAsync("/settings", cancellationToken);

        //     return result;
        // }

        /// <summary>
        /// call the dns and return the dns response
        /// </summary>
        /// <returns>Dns Srv response</returns>
        protected virtual async Task<IEnumerable<ServiceHostEntry>> ResolveDsnSvrUriAsync(string tcpAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dnsList = await DnsSrvClient.ResolveServiceAsync(tcpAddress, "api", System.Net.Sockets.ProtocolType.Tcp);
            DnsSrvFind = (dnsList.Length > 0);

            return dnsList;
        }

        /// <summary>
        /// Check the address using a get call
        /// </summary>
        /// <returns>bool, Success or fail</returns>
        // protected async Task<bool> CheckDnsSrvUrlAsync(string uri, CancellationToken cancellationToken = default(CancellationToken))
        // {
        //     HttpResponseMessage response = await GetAsync(uri, cancellationToken);

        //     return response.IsSuccessStatusCode;
        // }

        /// <summary>
        /// build a dns list sort by priority and balanced by weight
        /// </summary>
        /// <returns>qarnot address</returns>
        protected void CreateDnsList(IEnumerable<ServiceHostEntry> addressGiven, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<ServiceHostEntry> sortAddresses = SortByPriorityThemWeight(addressGiven);

            DsnList = sortAddresses.Select(service => new Address(service)).ToList();
            if (DsnList.Count > 0)
            {
                DsnCurrentAddress = DsnList.First();
            }
            else
            {
                DsnCurrentAddress = null;
            }
            // test addresses get
            // usefull ?
            // ?
            // foreach (var address in sortAddresses)
            // {
            //     var testUri = "https://" + address.HostName;
            //     if (await CheckDnsSrvUrlAsync(testUri, cancellationToken))
            //     {
            //         return testUri;
            //     }
            // }
        }

        /// <summary>
        /// Get qarnot tcp dns srv api url.
        /// </summary>
        /// <param name="uri">Api Url</param>
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
        /// resolve it, sort it and test the addresses
        /// return the first good address found or the uri given
        /// </summary>
        protected async Task BuildDsnSvrListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (DnsTcpUrl != null)
            {
                var addressGiven = await ResolveDsnSvrUriAsync(DnsTcpUrl, cancellationToken);
                CreateDnsList(addressGiven, cancellationToken);
            }
        }
    }
}