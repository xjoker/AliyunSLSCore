//
// ProjectDetailInfo.cs
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

namespace Aliyun.Api.LogService.Domain.Project
{
    public class ProjectDetailInfo : ProjectInfo
    {
        public ProjectDetailInfo(string projectName, ProjectState status, string description, string region,
            string owner, string createTime, string lastModifyTime)
            : base(projectName, status, description)
        {
            Region = region;
            Owner = owner;
            CreateTime = createTime;
            LastModifyTime = lastModifyTime;
        }

        /// <summary>
        ///     Project所在区域。
        /// </summary>
        public string Region { get; }

        /// <summary>
        ///     Project的所有者。
        /// </summary>
        public string Owner { get; }

        /// <summary>
        ///     Project创建时间。
        /// </summary>
        public string CreateTime { get; }

        /// <summary>
        ///     Project最后修改时间。
        /// </summary>
        public string LastModifyTime { get; }
    }
}