//
// GetShipperRequest.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shipper
{
    public class GetShipperRequest : ProjectScopedRequest
    {
        public const int DefaultOffset = 0;

        public const int DefaultSize = 100;

        public GetShipperRequest(string logstoreName, string shipperName, int from, int to)
        {
            LogstoreName = logstoreName;
            ShipperName = shipperName;
            From = from;
            To = to;
        }

        /// <summary>
        ///     日志库名称，同一 Project 下唯一。
        /// </summary>
        public string LogstoreName { get; }

        /// <summary>
        ///     日志投递规则名称，同一 Logstore 下唯一。
        /// </summary>
        public string ShipperName { get; }

        /// <summary>
        ///     日志投递任务创建时间区间。
        /// </summary>
        public int From { get; }

        /// <summary>
        ///     日志投递任务创建时间区间。
        /// </summary>
        public int To { get; }

        /// <summary>
        ///     默认为空，表示返回所有状态的任务，目前支持 success/fail/running 等状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     返回指定时间区间内投递任务的起始数目，默认值为 0。
        /// </summary>
        public int Offset { get; set; } = DefaultOffset;

        /// <summary>
        ///     返回指定时间区间内投递任务的数目，默认值为 100，最大为 500。
        /// </summary>
        public int Size { get; set; } = DefaultSize;
    }
}