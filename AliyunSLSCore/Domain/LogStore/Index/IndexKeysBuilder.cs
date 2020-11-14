//
// IndexKeysBuilder.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Aliyun.Api.LogService.Utils;

namespace Aliyun.Api.LogService.Domain.LogStore.Index
{
    public class IndexKeysBuilder
    {
        private readonly bool allowJson;

        private readonly IDictionary<string, IndexKeyInfo> keys = new Dictionary<string, IndexKeyInfo>();

        public IndexKeysBuilder() : this(true)
        {
            // Empty constructor.
        }

        private IndexKeysBuilder(bool allowJson)
        {
            this.allowJson = allowJson;
        }

        public IndexKeysBuilder AddText(string key, params char[] token)
        {
            return AddText(key, token.AsEnumerable());
        }

        public IndexKeysBuilder AddText(string key, IEnumerable<char> token, bool? caseSensitive = null,
            bool? chn = null)
        {
            var frozenToken = token?.ToArray(); // Avoid recalculate the non-reentranceable IEnumerable.
            Ensure.NotEmpty(frozenToken, nameof(token));

            var textKeyInfo = new IndexTextKeyInfo(frozenToken)
            {
                CaseSensitive = caseSensitive,
                Chn = chn
            };

            keys.Add(key, textKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddLong(string key, bool? docValue = null, string alias = null)
        {
            var longKeyInfo = new IndexLongKeyInfo
            {
                DocValue = docValue,
                Alias = alias
            };

            keys.Add(key, longKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddDouble(string key, bool? docValue = null, string alias = null)
        {
            var doubleKeyInfo = new IndexDoubleKeyInfo
            {
                DocValue = docValue,
                Alias = alias
            };

            keys.Add(key, doubleKeyInfo);

            return this;
        }

        public IndexKeysBuilder AddJson(string key, int maxDepth, params char[] token)
        {
            return AddJson(key, token, maxDepth);
        }

        public IndexKeysBuilder AddJson(string key, IEnumerable<char> token, int maxDepth,
            bool? chn = null, bool? indexAll = null, Action<IndexKeysBuilder> jsonKeys = null)
        {
            if (!allowJson) throw new InvalidOperationException("json index info is not support in current state.");

            var frozenToken = token?.ToArray(); // Avoid recalculate the non-reentranceable IEnumerable.
            Ensure.NotEmpty(frozenToken, nameof(token));

            IDictionary<string, IndexKeyInfo> subKeys;
            if (jsonKeys != null)
            {
                var subBuilder = new IndexKeysBuilder(false);
                jsonKeys(subBuilder);
                subKeys = subBuilder.Build();
            }
            else
            {
                subKeys = null;
            }

            var jsonKeyInfo = new IndexJsonKeyInfo(frozenToken, maxDepth)
            {
                Chn = chn,
                IndexAll = indexAll,
                JsonKeys = subKeys
            };

            keys.Add(key, jsonKeyInfo);

            return this;
        }

        public IDictionary<string, IndexKeyInfo> Build()
        {
            return new Dictionary<string, IndexKeyInfo>(keys);
        }
    }
}