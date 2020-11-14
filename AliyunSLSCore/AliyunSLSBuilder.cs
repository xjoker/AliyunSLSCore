using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AliyunSLSCore
{
    public class AliyunSlsBuilder : IAliyunSLSLog
    {
        private readonly string name;

        public ILogServiceClient Client;

        // 日志配置
        private AliyunSLSOptions options;

        public AliyunSlsBuilder(AliyunSLSOptions configModel, string name)
        {
            options = configModel ?? throw new ArgumentNullException(nameof(configModel));
            this.name = name;
            Client = LogServiceClientBuilders.HttpBuilder
                .Endpoint(options.Endpoint, options.ProjectName)
                .Credential(options.AccessKey, options.AccessSecret)
                .RequestTimeout(10000)
                .Build();
        }

        public void Dispose()
        {
            Client = null;
            options = null;
        }

        /// <summary>
        ///     日志发送基础方法
        /// </summary>
        /// <param name="keyValuePairs">键值对</param>
        public async void BaseLogSend(Dictionary<string, string> keyValuePairs)
        {
            try
            {
                if (keyValuePairs == null || !keyValuePairs.Any()) return;
                if (!options.Enable) return;
                if (Client == null) return;
                var list = new List<LogInfo>();
                var logItem = new LogInfo
                {
                    Time = DateTimeOffset.Now
                };
                foreach (var keyValuePair in keyValuePairs)
                    if (!string.IsNullOrEmpty(keyValuePair.Key) && !string.IsNullOrEmpty(keyValuePair.Value))
                        logItem.Contents.Add(keyValuePair.Key, keyValuePair.Value);
                list.Add(logItem);

                var logGroupInfo = new LogGroupInfo
                {
                    Topic = name ?? "",
                    Logs = list,
                    Source =
                        $"[{(options.IsDevelopmentModel ? options.DevelopmentSourcePrefix : options.OnlineSourcePrefix)}]{options.SourceName}"
                };

                await Client.PostLogStoreLogsAsync(options.LogStoreName, logGroupInfo);
            }
            catch (Exception)
            {
                // 作为日志模块不抛出异常
            }
        }

        /// <summary>
        ///     日志发送基础方法
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="m"></param>
        /// <param name="msg">消息体</param>
        /// <param name="ex">异常</param>
        public void BaseLogSend(LogLevel logLevel, MethodBase m = null, string msg = null, Exception ex = null)
        {
            if (!IsEnabled(logLevel, m)) return;

            var keyValuePairs = new Dictionary<string, string>
            {
                {"Level", logLevel.ToString()},
                {"Exception", ex != null ? ex.ToString() : ""},
                {"Message", msg},
                {"Class", m?.ReflectedType?.Name},
                {"Method", m?.Name}
            };
            BaseLogSend(keyValuePairs);
        }

        /// <summary>
        ///     日志写入
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="m">MethodBase</param>
        /// <param name="msg">消息体</param>
        /// <param name="ex">异常</param>
        public void WriteLog(LogLevel logLevel, string msg = null, MethodBase m = null, Exception ex = null)
        {
            // 未传入时尝试直接获取
            try
            {
                if (m == null)
                {
                    m = new StackTrace().GetFrame(1).GetMethod();
                }
            }
            catch
            {
                // ignored
            }

            BaseLogSend(logLevel, m, msg, ex);
        }

        public bool IsEnabled(LogLevel logLevel, MethodBase m = null)
        {
            if (logLevel < options.LogLevel) return false;

            if (options.IgoneClassList != null && options.IgoneClassList.Any() && !string.IsNullOrEmpty(m?.ReflectedType?.FullName))
            {
                if (options.IgoneClassList.Contains(m.ReflectedType?.FullName)) return false;
            }

            return true;
        }
    }
}