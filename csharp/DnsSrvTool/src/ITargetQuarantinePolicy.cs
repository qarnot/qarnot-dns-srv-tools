namespace DnsSrvTool
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Quarantine policy.
    /// </summary>
    public interface ITargetQuarantinePolicy
    {
        /// <summary>
        /// Gets the Quarantine Time.
        /// </summary>
        /// <value>Quarantine Time.</value>
        TimeSpan QuarantineDuration { get; }

        /// <summary>
        /// Check if the response should be set in quarantine.
        /// </summary>
        /// <param name="response">Response object.</param>
        /// <returns>Should be set in quarantine or not.</returns>
        bool ShouldQuarantine(HttpResponseMessage response);
    }
}