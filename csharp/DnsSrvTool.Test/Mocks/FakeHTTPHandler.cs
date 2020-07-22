namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1305, CA1303, CA2227, CA1304, CA1822, CA1307,

    public class FakeHTTPHandler : HttpClientHandler
    {
        private int ReturnMessageListIndex = 0;

        private int ReturnStatusCodeListIndex = 0;

        public FakeHTTPHandler()
            : base()
        {
            ReturnMessageList = null;
            ReturnMessageDictionary = null;
            ReturnStatusCodeList = null;
        }

        public Uri UrlCall { get; private set; } = null;

        public string ReturnMessage { get; set; } = "{\"Your\":\"response\"}";

        public List<System.Net.HttpStatusCode> ReturnStatusCodeList { get; set; }

        public List<string> ReturnMessageList { get; set; }

        /// <summary>
        /// key: url call
        /// value response message
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> ReturnMessageDictionary { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)).ConfigureAwait(false);
            var message = string.Empty;
            UrlCall = request?.RequestUri;

            if (ReturnStatusCodeList != null && ReturnStatusCodeList.Count > 0)
            {
                response.StatusCode = ReturnStatusCodeList[ReturnStatusCodeListIndex % ReturnStatusCodeList.Count];
                ReturnStatusCodeListIndex += 1;
            }

            if (ReturnMessageDictionary != null && ReturnMessageDictionary.ContainsKey(UrlCall?.ToString()))
            {
                message = ReturnMessageDictionary[UrlCall?.ToString()];
            }
            else if (ReturnMessageList != null && ReturnMessageList.Count > 0)
            {
                message = ReturnMessageList[ReturnMessageListIndex % ReturnMessageList.Count];
                ReturnMessageListIndex += 1;
            }
            else
            {
                message = ReturnMessage;
            }

            response.Content = new StringContent(message, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
