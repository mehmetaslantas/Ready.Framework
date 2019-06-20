using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ready.Framework.Configuration;

namespace Ready.Framework.Logging
{
    public class RedisLogger
    {
        public static void Info(LogModel model, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerPath = null)
        {
            model.EventType = "INFO";
            Log(model, lineNumber, caller, callerPath);
        }

        public static void Warn(LogModel model, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerPath = null)
        {
            model.EventType = "WARN";
            Log(model, lineNumber, caller, callerPath);
        }

        public static void Error(LogModel model, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerPath = null)
        {
            model.EventType = "ERROR";
            Log(model, lineNumber, caller, callerPath);
        }

        private static void ErrorWithDetails(Exception ex, object client, object user, string messageDetail = "", object extraData = null, int lineNumber = 0, string caller = null, string callerPath = null)
        {
            var model = new LogModel
            {
                EventType = "ERROR",
                Client = client,
                Exception = ex,
                User = user,
                CodeCaller = caller,
                CodeLineNumber = lineNumber,
                CodeCallerPath = callerPath,
                Message = ex.Message,
                MachineName = Environment.MachineName,
                Extra = extraData,
                MessageDetail = messageDetail,
                Code = ex.Message,
                Priority = "NORMAL"
            };
            Log(model, lineNumber, caller, callerPath);
        }

        public static void Error(Exception ex, object client, object user, string messageDetail = "", object extraData = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null,
            [CallerFilePath] string callerPath = null)
        {
            ErrorWithDetails(ex, client, user, messageDetail, extraData, lineNumber, caller, callerPath);
        }

        public void Debug(LogModel model, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string callerPath = null)
        {
            model.EventType = "DEBUG";
            Log(model, lineNumber, caller, callerPath);
        }

        private static void Log(LogModel model, int lineNumber, string caller, string callerPath)
        {
            if (!ConfigurationManager.GetParameter("AllowErrorLog", true))
                return;

            if (model == null)
                return;

            model.CreationDate = DateTime.UtcNow;
            model.MachineName = Environment.MachineName;
            model.CodeCaller = caller;
            model.CodeCallerPath = callerPath;
            model.CodeLineNumber = lineNumber;
            model.Environment = ConfigurationManager.ApplicationEnvironment;

            string message;

            try
            {
                message = JsonConvert.SerializeObject(model, JsonSettingsForLog);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var channelName = ConfigurationManager.GetParameter("RedisLogChannelName");
                        Db.Publish(channelName, message);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            catch
            {
                // ignored
            }
        }

        #region Redis Connection

        private static readonly JsonSerializerSettings JsonSettingsForLog = new JsonSerializerSettings
        {
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection =
            new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(ConfigurationManager.GetParameter("RedisCacheServerForLog") + ",syncTimeout=1000,connectTimeout=5000,abortConnect=false"));

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        private static IDatabase Db => Connection.GetDatabase();

        #endregion
    }
}