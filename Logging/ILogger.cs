using System;

namespace Ready.Framework.Logging
{
    public interface ILogger
    {
        void Log(string message, string source = null, string target = null, object parameters = null, object result = null);

        void Error(Exception ex, string extraMessage = null);
    }
}