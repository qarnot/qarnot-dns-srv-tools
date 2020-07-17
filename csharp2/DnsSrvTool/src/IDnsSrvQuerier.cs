namespace DnsSrvTool
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDnsSrvQuerier
    {
        Task<DnsSrvQueryResult> QueryServiceAsync(DnsSrvServiceDescription service);
    }
}