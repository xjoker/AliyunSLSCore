//
// GetMachineGroupResult.cs
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
    public class GetMachineGroupResult
    {
        public GetMachineGroupResult(string groupName, string groupType, MachineGroupAttributeInfo groupAttribute,
            string machineIdentifyType,
            IEnumerable<string> machineList, int createTime, int lastModifyTime)
        {
            GroupName = groupName;
            GroupType = groupType;
            GroupAttribute = groupAttribute;
            MachineIdentifyType = machineIdentifyType;
            MachineList = machineList;
            CreateTime = createTime;
            LastModifyTime = lastModifyTime;
        }

        /// <summary>
        ///     机器分组名称。
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        ///     机器分组类型，默认为空。
        /// </summary>
        public string GroupType { get; set; }

        /// <summary>
        ///     机器分组的属性，默认为空。
        /// </summary>
        public MachineGroupAttributeInfo GroupAttribute { get; }

        /// <summary>
        ///     机器标识类型，分为 IP 和 userdefined 两种。
        /// </summary>
        public string MachineIdentifyType { get; }

        /// <summary>
        ///     具体的机器标识，可以是 IP 或 userdefined-id。
        /// </summary>
        public IEnumerable<string> MachineList { get; }

        /// <summary>
        ///     机器分组创建时间。
        /// </summary>
        public int CreateTime { get; }

        /// <summary>
        ///     机器分组最近更新时间。
        /// </summary>
        public int LastModifyTime { get; }
    }
}