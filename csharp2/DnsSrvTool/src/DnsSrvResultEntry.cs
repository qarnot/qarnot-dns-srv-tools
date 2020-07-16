namespace DnsSrvTool
{
    using System;
    using System.Net;

#pragma warning disable CA1054, SA1611, CS1591
    public class DnsSrvResultEntry
    {
        public string HostName { get; }
        public int Port { get; }
        public int TimeToLiveInSec { get; } // usefull ?
        public int Priority { get; }
        public int Weight { get; }
        public DateTime CreationTime { get; }
        public DateTime TtlEndTime { get; }
        public DateTime QuarantineUntilTime { get; private set; }

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

        public bool IsAlive => TtlEndTime > DateTime.UtcNow;

        public bool IsAvailable => QuarantineUntilTime <= DateTime.UtcNow && IsAlive;

        public void ResetQuarantine()
        {
            QuarantineUntilTime = DateTime.UtcNow;
        }

        public void PutInQuarantine(TimeSpan quarnatineDuration)
        {
            QuarantineUntilTime = DateTime.UtcNow.Add(quarnatineDuration);
        }

        public DnsEndPoint DnsEndPoint => new DnsEndPoint(HostName, Port);
    }
}