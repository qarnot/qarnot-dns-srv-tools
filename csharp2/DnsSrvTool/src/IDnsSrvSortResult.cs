namespace DnsSrvTool
{
#pragma warning disable CA1054, SA1611, CS1591

    // Making requests, this is pure mechanism
    public interface IDnsSrvSortResult
    {
        DnsSrvQueryResult Sort(DnsSrvQueryResult result);
    }
}