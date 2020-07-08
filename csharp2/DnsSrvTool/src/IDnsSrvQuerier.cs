namespace DnsSrvTool
{
    using System.Threading;
    using System.Threading.Tasks;

        // Making requests, this is pure mechanism
    public interface IDnsSrvQuerier
    {
        Task<DnsSrvQueryResult> QueryService(DnsSrvServiceDescription service, int resultLifeTime);
    }
}