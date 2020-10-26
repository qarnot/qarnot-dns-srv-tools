namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307,

    public class FakeDnsSrvQuerierEmpty : IDnsSrvQuerier
    {
        public List<DnsSrvResultEntry> DnsSrvResultEntryList { get; } = new List<DnsSrvResultEntry>();

        public async Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service)
        {
            return await Task.FromResult(new DnsSrvQueryResult(DnsSrvResultEntryList));
        }
    }
}