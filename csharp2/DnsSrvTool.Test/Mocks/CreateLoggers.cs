namespace DnsSrvTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using DnsClient;
    using NLog;
    using NUnit.Framework;

    public class CreateLoggers
    {
        public static Microsoft.Extensions.Logging.ILogger CreateILoggerFromNLog(bool debug = false)
        {
            // Example of NLog build
            // https://stackoverflow.com/questions/56534730/nlog-works-in-asp-net-core-app-but-not-in-net-core-xunit-test-project
            // NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
            var configuration = new NLog.Config.LoggingConfiguration();
            configuration.AddRuleForAllLevels(new NLog.Targets.ConsoleTarget());
            NLog.Web.NLogBuilder.ConfigureNLog(configuration);

            // Create provider to bridge Microsoft.Extensions.Logging
            var provider = new NLog.Extensions.Logging.NLogLoggerProvider();

            // Create logger
            Microsoft.Extensions.Logging.ILogger logger = provider.CreateLogger(typeof(DnsSrvToolsIntergrationTest).FullName);

            // ILogger logger = NLog.LogManager.GetCurrentClassLogger();
            if (debug)
            {
                logger.LogDebug("This is a test of the log system : LogDebug.");
                logger.LogTrace("This is a test of the log system : LogTrace.");
                logger.LogInformation("This is a test of the log system : LogInformation.");
                logger.LogWarning("This is a test of the log system : LogWarning.");
                logger.LogError("This is a test of the log system : LogError.");
                logger.LogCritical("This is a test of the log system : LogCritical.");
            }

            return logger;
        }
    }
}