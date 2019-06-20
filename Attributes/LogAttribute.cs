using System;

namespace Ready.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogAttribute : Attribute
    {
        public bool LogErrors { get; set; }

        public bool LogEvents { get; set; }

        public LogAttribute(bool logErrors = false, bool logEvents = false)
        {
            LogErrors = logErrors;
            LogEvents = logEvents;
        }
    }
}