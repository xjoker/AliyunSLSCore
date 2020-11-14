using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AliyunSLSCore
{
    public interface IAliyunSLSLog
    {
        /// <summary>
        ///     日志发送基础方法
        /// </summary>
        /// <param name="keyValuePairs">键值对</param>
        void BaseLogSend(Dictionary<string, string> keyValuePairs);

        /// <summary>
        ///     日志发送基础方法
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="m"></param>
        /// <param name="msg">消息体</param>
        /// <param name="ex">异常</param>
        void BaseLogSend(LogLevel logLevel, MethodBase m = null, string msg = null, Exception ex = null);

        /// <summary>
        ///     日志写入
        /// </summary>
        /// <param name="logLevel">日志等级</param>
        /// <param name="m">MethodBase</param>
        /// <param name="msg">消息体</param>
        /// <param name="ex">异常</param>
        void WriteLog(LogLevel logLevel, string msg = null, MethodBase m = null, Exception ex = null);
    }
}