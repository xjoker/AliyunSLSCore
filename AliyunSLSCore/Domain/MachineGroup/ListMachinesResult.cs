//
// ListMachinesResult.cs
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

using System.Collections.Generic;

namespace Aliyun.Api.LogService.Domain.MachineGroup
{
    public class ListMachinesResult
    {
        public ListMachinesResult(int count, int total, IList<MachineInfo> machines)
        {
            Count = count;
            Total = total;
            Machines = machines;
        }

        /// <summary>
        ///     返回的 machinegroup 数目。
        /// </summary>
        public int Count { get; }

        /// <summary>
        ///     返回 machinegroup 总数。
        /// </summary>
        public int Total { get; }

        /// <summary>
        ///     返回的 machinegroup 名称列表。
        /// </summary>
        public IList<MachineInfo> Machines { get; }

        public class MachineInfo
        {
            public MachineInfo(string ip, string machineUniqueId, string userDefinedId, string lastHeartbeatTime)
            {
                Ip = ip;
                MachineUniqueId = machineUniqueId;
                UserDefinedId = userDefinedId;
                LastHeartbeatTime = lastHeartbeatTime;
            }

            /// <summary>
            ///     机器的 IP。
            /// </summary>
            public string Ip { get; }

            /// <summary>
            ///     机器 DMI UUID。
            /// </summary>
            public string MachineUniqueId { get; }

            /// <summary>
            ///     机器的用户自定义标识。
            /// </summary>
            public string UserDefinedId { get; }

            /// <summary>
            ///     机器最后的心跳时间。
            /// </summary>
            public string LastHeartbeatTime { get; }
        }
    }
}