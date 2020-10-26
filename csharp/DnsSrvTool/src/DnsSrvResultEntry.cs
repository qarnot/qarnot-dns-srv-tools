#pragma warning disable CA1303, CA1307, CA1304
namespace DnsSrvTool
{
    using System;
    using System.Net;

    /// <summary>
    /// A Dns entry value.
    /// </summary>
    public class DnsSrvResultEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSrvResultEntry"/> class.
        /// </summary>
        /// <param name="hostName">hostName.</param>
        /// <param name="port">port.</param>
        /// <param name="priority">priority.</param>
        /// <param name="weight">weight.</param>
        /// <param name="timeToLiveInSec">timeToLiveInSec.</param>
        public DnsSrvResultEntry(string hostName, int port, int priority, int weight, int timeToLiveInSec)
        {
            HostName = hostName;
            Port = port;
            Priority = priority;
            Weight = weight;
            TimeToLiveInSec = timeToLiveInSec;
            CreationTime = DateTime.UtcNow;
            TtlEndTime = DateTime.UtcNow.AddSeconds(TimeToLiveInSec);
            QuarantineUntilTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a DnsEndPoint of the entity.
        /// </summary>
        /// <returns>DnsEndPoint of the entity.</returns>
        public DnsEndPoint DnsEndPoint => new DnsEndPoint(HostName, Port);

        /// <summary>
        /// Gets a value indicating whether Time to live passed.
        /// </summary>
        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        /// <summary>
        ///  Gets a value indicating whether that the entry is alive and not in quarantine.
        /// </summary>
        public bool IsAvailable => QuarantineUntilTime <= DateTime.UtcNow && IsAlive;

        /// <summary>
        /// Gets Host name value.
        /// </summary>
        /// <value> Host name value.</value>
        public string HostName { get; }

        /// <summary>
        /// Gets Port value.
        /// </summary>
        /// <value> Port value.</value>
        public int Port { get; }

        /// <summary>
        /// Gets Time to live in second.
        /// </summary>
        /// <value> Time to live in second value.</value>
        public int TimeToLiveInSec { get; }

        /// <summary>
        /// Gets Priority value.
        /// </summary>
        /// <value> Priority value.</value>
        public int Priority { get; }

        /// <summary>
        /// Gets Weight value.
        /// </summary>
        /// <value> Weight value.</value>
        public int Weight { get; }

        /// <summary>
        /// Gets Creation date.
        /// </summary>
        /// <value> Creation date.</value>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Gets Life Time End Date.
        /// </summary>
        /// <value> Life Time End Date.</value>
        public DateTime TtlEndTime { get; }

        /// <summary>
        /// Gets Quarantine end date.
        /// </summary>
        /// <value> Quarantine end date.</value>
        public DateTime QuarantineUntilTime { get; private set; }

        /// <summary>
        /// Reset the quarantine time.
        /// </summary>
        public void ResetQuarantine()
        {
            QuarantineUntilTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Put the entity in quarantine.
        /// </summary>
        /// <param name="quarantineDuration">Quarantine duration.</param>
        public void PutInQuarantine(TimeSpan quarantineDuration)
        {
            QuarantineUntilTime = DateTime.UtcNow.Add(quarantineDuration);
        }

        /// <summary>
        /// Print the full Entity information.
        /// </summary>
        /// <param name="format">Add string inforation if a format is specified. format == "f" return a string with the full information.</param>
        /// <returns>String entity information.</returns>
        public string ToString(string format)
        {
            if (!string.IsNullOrEmpty(format) && format.ToLower() == "f")
            {
                return $"HostName: {HostName} Port: {Port} Priority: {Priority} Weight: {Weight} TimeToLiveInSec: {TimeToLiveInSec} CreationTime: {CreationTime} TtlEndTime: {TtlEndTime} QuarantineUntilTime: {QuarantineUntilTime}";
            }
            else
            {
                return this.ToString();
            }
        }

        /// <summary>
        /// Print the entity host and port information.
        /// </summary>
        /// <returns>String entity information.</returns>
        public override string ToString()
        {
            return $"{{Host: {HostName} Port: {Port}}}";
        }
    }
}