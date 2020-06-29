namespace DnsSrvTool
{
    /// <summary>
    /// IDnsSrvSortResult interface.
    /// </summary>
    public interface IDnsSrvSortResult
    {
        /// <summary>
        /// Sort a DnsSrvQueryResult.
        /// </summary>
        /// <param name="result">Result to be sort.</param>
        /// <returns>return the result object sorted.</returns>
        DnsSrvQueryResult Sort(DnsSrvQueryResult result);
    }
}