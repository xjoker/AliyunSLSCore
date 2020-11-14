//
// CreateConfigRequest.cs
//
// Author:
//       MiNG <developer@ming.gz.cn>
//
// Copyright (c) 2018 Alibaba Cloud
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Aliyun.Api.LogService.Domain.Project;

namespace Aliyun.Api.LogService.Domain.Config
{
    public class CreateConfigRequest : ProjectScopedRequest
    {
        public CreateConfigRequest(string configName, string inputType, ConfigInputDetailInfo inputDetail,
            string outputType, ConfigOutputDetailInfo outputDetail)
        {
            ConfigName = configName;
            InputType = inputType;
            InputDetail = inputDetail;
            OutputType = outputType;
            OutputDetail = outputDetail;
        }

        /// <summary>
        ///     日志配置名称， Project 下唯一。
        /// </summary>
        public string ConfigName { get; }

        /// <summary>
        ///     输入类型，现在只支持 file。
        /// </summary>
        public string InputType { get; }

        /// <summary>
        ///     输入详情。
        /// </summary>
        public ConfigInputDetailInfo InputDetail { get; }

        /// <summary>
        ///     输出类型，现在只支持 LogService。
        /// </summary>
        public string OutputType { get; }

        /// <summary>
        ///     输出详情。
        /// </summary>
        public ConfigOutputDetailInfo OutputDetail { get; }

        /// <summary>
        ///     Logtail 配置日志样例，最大支持 1000 字节。
        /// </summary>
        public string LogSample { get; set; }
    }
}