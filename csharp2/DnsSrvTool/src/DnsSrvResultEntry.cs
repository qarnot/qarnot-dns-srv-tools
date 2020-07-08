namespace DnsSrvTool
{
    using System;
    using System.Net;
    public class DnsSrvResultEntry
    {
        public string HostName { get; }
        public int Port { get; }
        public int TimeToLiveInSec { get; } // usefull ?
        public int Priority { get; }
        public int Weight { get; }
        public DateTime CreationTime { get; }
        public DateTime TtlEndTime { get; }
        public DateTime QuarantineTime { get; private set; }

        public DnsSrvResultEntry(string hostName, int port, int priority, int weight, int timeToLiveInSec)
        {
            HostName = hostName;
            Port = port;
            Priority = priority;
            Weight = weight;
            TimeToLiveInSec = timeToLiveInSec;
            CreationTime = DateTime.Now;
            TtlEndTime = DateTime.Now.AddSeconds(TimeToLiveInSec);
            QuarantineTime = DateTime.Now;
        }

        public bool IsAlive()
        {
            return TtlEndTime > DateTime.Now;
        }

        public bool IsAvailable()
        {
            return QuarantineTime <= DateTime.Now && IsAlive();
        }

        public void ResetQuarantine()
        {
            QuarantineTime = DateTime.Now;
        }

        public void PutInQuarantine(TimeSpan quarnatineDuration)
        {
            QuarantineTime = DateTime.Now.Add(quarnatineDuration);
        }

        public DnsEndPoint DnsEndPoint()
        {
            return new DnsEndPoint(HostName, Port);
        }
    }
}