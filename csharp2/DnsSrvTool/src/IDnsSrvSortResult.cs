namespace DnsSrvTool
{
    // Making requests, this is pure mechanism
    public interface IDnsSrvSortResult
    {
        DnsSrvQueryResult Sort(DnsSrvQueryResult result);
    }
}