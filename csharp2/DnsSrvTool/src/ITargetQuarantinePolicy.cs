namespace DnsSrvTool
{
    using System.Net.Http;

    public interface ITargetQuarantinePolicy
    {
        bool ShouldQuarantine(HttpResponseMessage response);
    }
}