#pragma warning disable CA1303, CA1307
namespace DnsSrvTool
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Quarantine entities Manager.
    /// </summary>
    public class TargetQuarantinePolicyServerUnavailable : ITargetQuarantinePolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetQuarantinePolicyServerUnavailable"/> class.
        /// </summary>
        /// <param name="quarantineDuration">Quarantine time.</param>
        public TargetQuarantinePolicyServerUnavailable(TimeSpan? quarantineDuration = null)
        {
            QuarantineDuration = quarantineDuration ?? new TimeSpan(0, 5, 0);
        }

        /// <summary>
        /// Gets the Quarantine Time.
        /// </summary>
        /// <value>Quarantine Time.</value>
        public TimeSpan QuarantineDuration { get; }

        /// <summary>
        /// Check if the response should be set in quarantine.
        /// </summary>
        /// <param name="response">Response object.</param>
        /// <returns>Should be set in quarantine or not.</returns>
        public bool ShouldQuarantine(HttpResponseMessage response)
        {
            switch (response?.StatusCode)
            {
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.GatewayTimeout:
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return true;
                default:
                    return false;
            }
        }
    }
}