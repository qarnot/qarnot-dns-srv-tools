

namespace QarnotDnsHandler
{
    using System;

    /// <summary>
    /// Constant class
    /// Values must not be alterated
    /// </summary>
    internal static class QEnvVariables
    {
        public const string QARNOT_DNS_CACHETIME = "QARNOT_DNS_CACHETIME";
        public const string QARNOT_DNS_FAILTIME = "QARNOT_DNS_FAILTIME";
        public const string QARNOT_DNS_RETRIEVETIME = "QARNOT_DNS_RETRIEVETIME";
        public const string QARNOT_DNS_SERVICE = "QARNOT_DNS_SERVICE";
        public const string QARNOT_DNS_DOMAIN = "QARNOT_DNS_DOMAIN";
        public const string QARNOT_DNS_PROTOCOL = "QARNOT_DNS_PROTOCOL";

        public static int? DnsCachetime { get { return GetIntegerEnvironmentVariable(QARNOT_DNS_CACHETIME); } }
        public static int? DnsFailTime { get { return GetIntegerEnvironmentVariable(QARNOT_DNS_FAILTIME); } }
        public static int? DnsRetrieve { get { return GetIntegerEnvironmentVariable(QARNOT_DNS_RETRIEVETIME); } }
        public static string DnsService { get { return Environment.GetEnvironmentVariable(QARNOT_DNS_SERVICE); } }
        public static string DnsDomain { get { return Environment.GetEnvironmentVariable(QARNOT_DNS_DOMAIN); } }

        /// <summary>
        /// return the QARNOT_DNS_PROTOCOL environment variable, see the format ire:
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.protocoltype?view=netcore-3.1
        /// </summary>
        /// <typeparam name="System.Net.Sockets.ProtocolType"></typeparam>
        /// <returns></returns>
        public static System.Net.Sockets.ProtocolType DnsProtocol { get { return GetEnumEnvironmentVariable<System.Net.Sockets.ProtocolType>(QARNOT_DNS_PROTOCOL); } }

        private static int? GetIntegerEnvironmentVariable(string environmentVariableName)
        {
            var envVariable = Environment.GetEnvironmentVariable(environmentVariableName);
            if (string.IsNullOrEmpty(envVariable))
                return null;

            return Int32.Parse(envVariable);
        }

        private static T GetEnumEnvironmentVariable<T>(string environmentVariableName) where T : struct, Enum
        {
            var envVariable = Environment.GetEnvironmentVariable(environmentVariableName);

            return (T)Enum.Parse(typeof(T), envVariable);
        }
    }
}
