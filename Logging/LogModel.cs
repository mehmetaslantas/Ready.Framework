using System;

namespace Ready.Framework.Logging
{
    public class LogModel
    {
        public string ApplicationName => "APAC_Mobile_V3";

        public string EventType { get; internal set; }

        public string CodeCaller { get; internal set; }

        public string CodeCallerPath { get; internal set; }

        public int CodeLineNumber { get; internal set; }

        public string MachineName { get; internal set; }

        public string Priority { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public string MessageDetail { get; set; }

        public object Exception { get; set; }

        public object User { get; set; }

        public object Client { get; set; }

        public object Extra { get; set; }

        public string Environment { get; internal set; }

        /// <summary>
        ///     yyyy-MM-ddTHH:mm:ss.fffffffzzz
        /// </summary>
        public DateTime CreationDate { get; internal set; }
    }
}