namespace DnsSrvTool
{
    using System;

    public interface IDnsServiceExtractor
    {
        DnsSrvServiceDescription FromUri(Uri uri);
    }
}