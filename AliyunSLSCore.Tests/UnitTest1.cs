using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AliyunSLSCore.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var config = new AliyunSLSOptions()
            {
                IsDevelopmentModel = Convert.ToBoolean(true),
                Endpoint = "rrr233.cn-hangzhou.log.aliyuncs.com",
                AccessSecret = "****",
                AccessKey = "****",
                LogStoreName = "rrr233",
                ProjectName = "rrr233",
                IgoneClassList = new List<string>()
                {
                    //"AliyunSLSCore.Tests.UnitTest1"
                }
            };

            var log = new AliyunSlsBuilder(config, "UnitTest1");

            log.WriteLog(LogLevel.Debug, "233333333333");

            Console.ReadLine();
        }
    }
}