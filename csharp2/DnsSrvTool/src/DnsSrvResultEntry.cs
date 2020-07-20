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
        /// Host name value.
        /// </summary>
        /// <value> Host name value</value>
        public string HostName { get; }

        /// <summary>
        /// Port value.
        /// </summary>
        /// <value> Port value</value>
        public int Port { get; }

        /// <summary>
        /// Time to live in second.
        /// </summary>
        /// <value> Time to live in second value</value>
        public int TimeToLiveInSec { get; }

        /// <summary>
        /// Priority value.
        /// </summary>
        /// <value> Priority value</value>
        public int Priority { get; }

        /// <summary>
        /// Weight value.
        /// </summary>
        /// <value> Weight value</value>
        public int Weight { get; }

        /// <summary>
        /// Creation date.
        /// </summary>
        /// <value> Creation date</value>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Life Time End Date.
        /// </summary>
        /// <value> Life Time End Date</value>
        public DateTime TtlEndTime { get; }

        /// <summary>
        /// Quarantine end date.
        /// </summary>
        /// <value> Quarantine end date</value>
        public DateTime QuarantineUntilTime { get; private set; }

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
        /// Time to live passed.
        /// </summary>
        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        /// <summary>
        /// Is alive and not in quarantine.
        /// </summary>
        public bool IsAvailable => QuarantineUntilTime <= DateTime.UtcNow && IsAlive;

        // Reset the quarantine time.
        public void ResetQuarantine()
        {
            QuarantineUntilTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Put the entity in quarantine.
        /// </summary>
        /// <param name="quarnatineDuration">Quarnatine duration.</param>
        public void PutInQuarantine(TimeSpan quarnatineDuration)
        {
            QuarantineUntilTime = DateTime.UtcNow.Add(quarnatineDuration);
        }

        /// <summary>
        /// Return a DnsEndPoint of the entity.
        /// </summary>
        /// <returns>DnsEndPoint of the entity</returns>
        public DnsEndPoint DnsEndPoint => new DnsEndPoint(HostName, Port);

        /// <summary>
        /// Print the full Entity information.
        /// </summary>
        /// <returns>String entity information.</returns>
        public string ToFullString()
        {
            return $"HostName: {HostName} Port: {Port} Priority: {Priority} Weight: {Weight} TimeToLiveInSec: {TimeToLiveInSec} CreationTime: {CreationTime} TtlEndTime: {TtlEndTime} QuarantineUntilTime: {QuarantineUntilTime}";
        }

        /// <summary>
        /// Print the entity host and port information.
        /// </summary>
        /// <returns>String entity information.</returns>
        public override string ToString()
        {
            return "{Host:" + $"{HostName}" + " Port:" + $"{Port}" + "}";
        }
    }
}