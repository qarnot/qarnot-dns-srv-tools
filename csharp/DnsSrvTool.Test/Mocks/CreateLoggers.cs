namespace DnsSrvTool.Test
{
    using Microsoft.Extensions.Logging;

#pragma warning disable CA1305, CA1303, CA1304, CA1822, CA1307, CA2000

    public static class CreateLoggers
    {
        public static Microsoft.Extensions.Logging.ILogger CreateILoggerFromNLog(bool debug = false)
        {
            // NLog build Example for microsoft ILogger find on :
            // https://stackoverflow.com/questions/56534730/nlog-works-in-asp-net-core-app-but-not-in-net-core-xunit-test-project
            var configuration = new NLog.Config.LoggingConfiguration();
            configuration.AddRuleForAllLevels(new NLog.Targets.ConsoleTarget());
            NLog.Web.NLogBuilder.ConfigureNLog(configuration);

            // Create provider to bridge Microsoft.Extensions.Logging
            var provider = new NLog.Extensions.Logging.NLogLoggerProvider();

            // Create logger
            Microsoft.Extensions.Logging.ILogger logger = provider.CreateLogger(typeof(DnsSrvToolsIntergrationTest).FullName);

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
