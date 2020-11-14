using Microsoft.Extensions.Logging;

namespace AliyunSLSCore
{
    public class AliyunSLSProvider : ILoggerProvider
    {
        private readonly AliyunSLSOptions options;

        public AliyunSLSProvider(AliyunSLSOptions options)
        {
            this.options = options;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AliyunSLSLogger(categoryName, options);
        }
    }
}