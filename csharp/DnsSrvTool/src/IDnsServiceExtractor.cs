namespace DnsSrvTool
{
    using System;

    /// <summary>
    /// create DnsSrvServiceDescription objects.
    /// </summary>
    public interface IDnsServiceExtractor
    {
        /// <summary>
        /// create DnsSrvServiceDescription form Uri.
        /// </summary>
        /// <param name="uri">Uri to be extract.</param>
        /// <returns>DnsSrvServiceDescription return object.</returns>
        DnsSrvServiceDescription FromUri(Uri uri);
    }
}