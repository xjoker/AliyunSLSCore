using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AliyunSLSCore
{
    public class AliyunSLSOptions
    {
        /// <summary>
        ///     是否启用阿里云日志
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        ///     是否为开发模式
        /// </summary>
        public bool IsDevelopmentModel { get; set; }

        /// <summary>
        ///     开发模式中的 Topic 提示语
        /// </summary>
        public string DevelopmentSourcePrefix { get; set; } = "Development";

        /// <summary>
        ///     线上模式提示语
        /// </summary>
        public string OnlineSourcePrefix { get; set; } = "Online";

        /// <summary>
        ///     忽略类名前缀，例如"AliyunSLSCore.Tests.UnitTest1",过滤后将不在出现在日志内
        /// </summary>
        public List<string> IgoneClassList { get; set; }

        /// <summary>
        ///     阿里云api访问key
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        ///     阿里云api访问密钥
        /// </summary>
        public string AccessSecret { get; set; }

        /// <summary>
        ///     阿里云api访问地址
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        ///     阿里云日志项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        ///     阿里云日志存储名称
        /// </summary>
        public string LogStoreName { get; set; }

        /// <summary>
        ///     日志记录级别
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        ///     来源名称
        /// </summary>
        public string SourceName { get; set; }
    }
}