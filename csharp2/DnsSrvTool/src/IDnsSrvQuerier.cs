namespace DnsSrvTool
{
    using System.Threading;
    using System.Threading.Tasks;

#pragma warning disable CA1054, SA1611, CS1591
        // Making requests, this is pure mechanism
    public interface IDnsSrvQuerier
    {
        Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service);
    }
}