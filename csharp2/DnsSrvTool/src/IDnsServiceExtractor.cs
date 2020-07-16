namespace DnsSrvTool
{
    using System;

#pragma warning disable CA1054, SA1611, CS1591
    public interface IDnsServiceExtractor
    {
        DnsSrvServiceDescription FromUri(Uri uri);
    }
}