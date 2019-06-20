using System;

namespace Ready.Framework.Configuration
{
    public struct ParameterResult<T>
    {
        public T Parameter { get; set; }

        public bool Success { get; set; }
    }

    public interface IConfigurationService
    {
        ParameterResult<T> GetParameter<T>(string key, T defaultValue = default(T)) where T : struct, IConvertible;

        ParameterResult<string> GetParameter(string key, string defaultValue = "");
    }
}