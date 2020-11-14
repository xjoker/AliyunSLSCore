//
// IndexKeyInfo.cs
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
using Aliyun.Api.LogService.Utils;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Domain.LogStore.Index
{
    /*****************************************
     * TODO: Remove the coupling of Json.NET *
     *****************************************/

    public class IndexKeyInfo
    {
        public IndexKeyInfo(string type)
        {
            Type = type;
        }

        /// <summary>
        ///     字段类型，目前支持text，long，double和json等四种。
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     是否支持统计分析，默认值为false，标识不支持。
        /// </summary>
        [JsonProperty("doc_value")]
        public bool? DocValue { get; set; }

        /// <summary>
        ///     查询别名，默认为空。
        /// </summary>
        public string Alias { get; set; }
    }

    public class IndexTextKeyInfo : IndexKeyInfo
    {
        public IndexTextKeyInfo(IEnumerable<char> token)
            : base("text")
        {
            Token = token.Freeze();
        }

        /// <summary>
        ///     是否区分大小写，默认值为false，表示不区分。
        /// </summary>
        public bool? CaseSensitive { get; set; }

        /// <summary>
        ///     分词字符列表，只支持单个英文字符。
        /// </summary>
        public IEnumerable<char> Token { get; }

        /// <summary>
        ///     是否进行中文分词，默认值为false，表示不进行中文分词。
        /// </summary>
        public bool? Chn { get; set; }
    }

    public class IndexLongKeyInfo : IndexKeyInfo
    {
        public IndexLongKeyInfo() : base("long")
        {
            // Empty constructor.
        }
    }

    public class IndexDoubleKeyInfo : IndexKeyInfo
    {
        public IndexDoubleKeyInfo() : base("double")
        {
            // Empty constructor.
        }
    }

    public class IndexJsonKeyInfo : IndexKeyInfo
    {
        public IndexJsonKeyInfo(IEnumerable<char> token, int maxDepth)
            : base("json")
        {
            Token = token.Freeze();
            MaxDepth = maxDepth;
        }

        /// <summary>
        ///     分词字符列表，只支持单个英文字符。
        /// </summary>
        public IEnumerable<char> Token { get; }

        /// <summary>
        ///     是否进行中文分词，默认值为false，表示不进行中文分词。
        /// </summary>
        public bool? Chn { get; set; }

        /// <summary>
        ///     是否对所有json key进行索引，默认为否。
        /// </summary>
        [JsonProperty("index_all")]
        public bool? IndexAll { get; set; }

        /// <summary>
        ///     表示解析json的最大深度。
        /// </summary>
        [JsonProperty("max_depth")]
        public int MaxDepth { get; }

        /// <summary>
        ///     表示json中具体key的索引属性。 其中object中每个item支持text，long和double三种类型，属性对应类型一致。
        /// </summary>
        [JsonProperty("json_keys")]
        public IDictionary<string, IndexKeyInfo> JsonKeys { get; set; }
    }
}