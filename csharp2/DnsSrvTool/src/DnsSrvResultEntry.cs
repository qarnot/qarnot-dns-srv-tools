namespace DnsSrvTool
{
    using System;
    using System.Net;

    public class DnsSrvResultEntry
    {
        public string HostName { get; }
        public int Port { get; }
        public int TimeToLiveInSec { get; }
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


        public string ToFullString()
        {
            return $"HostName: {HostName} Port: {Port} Priority: {Priority} Weight: {Weight} TimeToLiveInSec: {TimeToLiveInSec} CreationTime: {CreationTime} TtlEndTime: {TtlEndTime} QuarantineUntilTime: {QuarantineUntilTime}";
        }

        public override string ToString()
        {
            return "{Host:" + $"{HostName}" + " Port:" + $"{Port}" + "}";
        }
    }
}