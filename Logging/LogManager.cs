using System;
using System.Collections.Generic;

namespace Ready.Framework.Logging
{
    public static class LogManager
    {
        private static List<ILogger> LogServices { get; } = new List<ILogger>();

        public static void AddLogService(ILogger logger)
        {
            LogServices.Add(logger);
        }

        public static void Log(string message, string source = null, string target = null, object parameters = null, object result = null)
        {
            foreach (var logService in LogServices)
                logService.Log(message, source, target, parameters, result);
        }

        public static void Error(Exception ex, string extraMessage = null)
        {
            foreach (var logService in LogServices)
                logService.Error(ex, extraMessage);
        }
    }
}