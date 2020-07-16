namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeDnsSrvQuerier : IDnsSrvQuerier
    {
        public List<DnsSrvResultEntry> DnsSrvResultEntryList { get; } = new List<DnsSrvResultEntry>()
            {
                new DnsSrvResultEntry("api.qarnot1.com", 430, 1, 10, 20),
                new DnsSrvResultEntry("api.qarnot2.com", 430, 2, 10, 20),
                new DnsSrvResultEntry("api.qarnot3.com", 430, 3, 10, 20),
                new DnsSrvResultEntry("api.qarnot4.com", 430, 4, 10, 20),
                new DnsSrvResultEntry("api.qarnot5.com", 430, 5, 10, 20),
            };

        public async Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service)
        {
            return await Task.FromResult(new DnsSrvQueryResult(DnsSrvResultEntryList));
        }
    }

    public class FakeDnsSrvQuerierEmpty : IDnsSrvQuerier
    {
        public List<DnsSrvResultEntry> DnsSrvResultEntryList { get; } = new List<DnsSrvResultEntry>();

        public async Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service)
        {
            return await Task.FromResult(new DnsSrvQueryResult(DnsSrvResultEntryList));
        }
    }
}