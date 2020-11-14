//
// ShardInfo.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shard
{
    public class ShardInfo
    {
        public ShardInfo(int shardId, ShardState status, string inclusiveBeginKey, string exclusiveEndKey,
            long createTime)
        {
            ShardId = shardId;
            Status = status;
            InclusiveBeginKey = inclusiveBeginKey;
            ExclusiveEndKey = exclusiveEndKey;
            CreateTime = createTime;
        }

        /// <summary>
        ///     Shard ID，分区号。
        /// </summary>
        public int ShardId { get; }

        /// <summary>
        ///     分区的状态。
        ///     <list type="bullet">
        ///         <item>
        ///             <description>readwrite：可以读写</description>
        ///         </item>
        ///         <item>
        ///             <description>readonly：只读数据</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public ShardState Status { get; }

        /// <summary>
        ///     分区起始的Key值，分区范围中包含该Key值。
        /// </summary>
        public string InclusiveBeginKey { get; }

        /// <summary>
        ///     分区结束的Key值，分区范围中不包含该Key值。
        /// </summary>
        public string ExclusiveEndKey { get; }

        /// <summary>
        ///     分区创建时间。
        /// </summary>
        public long CreateTime { get; }
    }
}