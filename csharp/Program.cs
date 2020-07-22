namespace DnsSrvTool
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Net.Sockets;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;

    // First, we have a class that can make DNS SRV requests and wrap
    // the result is something that does not depend on the underlying library
    // Some data classes











    // We will also be able to parametrize the policy of putting a host in quarantine.
    // This is HTTP specific, as this can only be application-specific.





    // Now we can start using the class for http(s) building a handler for HttpClient.
    // Notice that until this point:
    //      - Qarnot was never mentioned
    //      - application protocols were never mentioned
    //      - even the concept of an URI was not mentioned.
    //
    // All that is above could be passed to http, amqp, sql, maybe even ping, ...





    // NOTE: there is still no mention of Qarnot or any convention that we have to find
    //       the service name and domain from the base request URI. This is still fully
    //       generic and could be used by anyone.

    // Now, we'll make some helpers implementing conventions internal to Qarnot






    // Then we can create other ones if we want to have some special tweaking for
    // some particular environment or some particular product. But the core logic
    // will remains the same, it's all pluggable.

}
