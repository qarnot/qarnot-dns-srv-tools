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
        public async Task<DnsSrvQueryResult> QueryService(DnsSrvServiceDescription service, int resultLifeTime)
        {
            var DnsSrvResultEntryList =  new List<DnsSrvResultEntry>()
            {
                new DnsSrvResultEntry("api.qarnot1.com", 430, 4, 10, 20),
                new DnsSrvResultEntry("api.qarnot1.com", 430, 6, 10, 20),
                new DnsSrvResultEntry("api.qarnot1.com", 430, 1, 10, 20),
                new DnsSrvResultEntry("api.qarnot1.com", 430, 3, 10, 20),
                new DnsSrvResultEntry("api.qarnot1.com", 430, 2, 10, 20),
            };
            return new DnsSrvQueryResult(DnsSrvResultEntryList, resultLifeTime);
        }

    }
}