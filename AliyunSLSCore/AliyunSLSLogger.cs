using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AliyunSLSCore
{
    public class AliyunSLSLogger : ILogger
    {
        private readonly IAliyunSLSLog logger;
        private readonly string name;

        public AliyunSLSLogger(string name, AliyunSLSOptions options)
        {
            this.name = string.IsNullOrEmpty(name) ? nameof(AliyunSLSLogger) : name;
            if (options == null) throw new ArgumentNullException(nameof(options));
            logger = new AliyunSlsBuilder(options, this.name);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message)) return;

            var keyValuePairs = new Dictionary<string, string>
            {
                {"categoryName", name},
                {"Level", logLevel.ToString()},
                {"Exception", exception != null ? exception.ToString() : ""},
                {"Message", message}
            };

            logger.BaseLogSend(keyValuePairs);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}