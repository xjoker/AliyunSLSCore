//
// HttpResponseMessageResolver.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Utils;
using Ionic.Zlib;
using LZ4;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public class HttpResponseMessageResolver : IResponseResolver
    {
        private Func<byte[], byte[]> decompressor;

        private Func<byte[], Type, object> deserializer;

        public HttpResponseMessageResolver(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;

            decompressor = AutoDecompressContent;
            deserializer = AutoDeserializeContent;
        }

        public HttpResponseMessage HttpResponseMessage { get; }

        public string RequestId { get; private set; }

        public bool IsSuccess { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public IDictionary<string, string> Headers { get; private set; }

        public IResponseResolver<TResult> With<TResult>()
            where TResult : class
        {
            return new TypedWrapper<TResult>(this);
        }

        public IResponseResolver Decompress(Func<byte[], byte[]> decompressor)
        {
            this.decompressor = decompressor ?? throw new ArgumentNullException(nameof(decompressor));
            return this;
        }

        public IResponseResolver<TResult> Deserialize<TResult>(Func<byte[], TResult> deserializer) where TResult : class
        {
            if (deserializer == null) throw new ArgumentNullException(nameof(deserializer));

            this.deserializer = (data, resultType) =>
            {
                var bindType = typeof(TResult);
                if (bindType != resultType)
                    throw new ArgumentException(
                        $"Type mismatch, binding type: [{bindType}], actual type: [{resultType}]", nameof(TResult));

                return deserializer(data);
            };

            return new TypedWrapper<TResult>(this);
        }

        public async Task<IResponse> ResolveAsync()
        {
            ResolveInternal();

            var readOnlyHeaders = new ReadOnlyDictionary<string, string>(Headers);

            var error = IsSuccess ? null : await HttpResponseMessage.Content.ReadAsAsync<Error>();

            return new HttpResponse(HttpResponseMessage, IsSuccess, StatusCode, RequestId, readOnlyHeaders, error);
        }

        public async Task<IResponse<TResult>> ResolveAsync<TResult>() where TResult : class
        {
            ResolveInternal();

            var readOnlyHeaders = new ReadOnlyDictionary<string, string>(Headers);

            if (!IsSuccess)
            {
                var error = await HttpResponseMessage.Content.ReadAsAsync<Error>();
                return new HttpResponse<TResult>(HttpResponseMessage, IsSuccess, StatusCode, RequestId, readOnlyHeaders,
                    error);
            }

            var result = await ResolveResultAsync<TResult>();

            return new HttpResponse<TResult>(HttpResponseMessage, IsSuccess, StatusCode, RequestId, readOnlyHeaders,
                result);
        }

        public static IResponseResolver For(HttpResponseMessage httpResponseMessage)
        {
            return new HttpResponseMessageResolver(httpResponseMessage);
        }

        public static IResponseResolver<TResult> For<TResult>(HttpResponseMessage httpResponseMessage)
            where TResult : class
        {
            return new HttpResponseMessageResolver(httpResponseMessage).With<TResult>();
        }

        #region Deserialize

        private object AutoDeserializeContent(byte[] data, Type resultType)
        {
            // Content negotiate is not supported.
            using (var stream = new MemoryStream(data, false))
            using (var textReader = new StreamReader(stream, Encoding.UTF8 /*TODO: Hard code*/))
            {
                return JsonSerializer.CreateDefault().Deserialize(textReader, resultType);
            }
        }

        #endregion Deserialize

        private void ResolveInternal()
        {
            if (HttpResponseMessage.Headers.TryGetValues(LogHeaders.RequestId, out var requestIds))
                RequestId = requestIds.FirstOrDefault(); // Fault tolerance.
            IsSuccess = HttpResponseMessage.IsSuccessStatusCode;
            StatusCode = HttpResponseMessage.StatusCode;

            Headers = HttpResponseMessage.Headers
                .Concat(HttpResponseMessage.Content.Headers ??
                        Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault() /* Fault tolerance */);
        }

        private async Task<TResult> ResolveResultAsync<TResult>()
            where TResult : class
        {
            var httpContent = HttpResponseMessage.Content;
            if (httpContent == null) return null;

            var data = await httpContent.ReadAsByteArrayAsync();
            if (data.IsEmpty()) return null;

            data = decompressor(data);
            var result = deserializer(data, typeof(TResult));

            return (TResult) result; // Always safe! Expect the custom serializer does some weird operations.
        }

        private class TypedWrapper<TResult> : IResponseResolver<TResult>
            where TResult : class
        {
            private readonly HttpResponseMessageResolver innerResolver;

            internal TypedWrapper(HttpResponseMessageResolver innerResolver)
            {
                this.innerResolver = innerResolver;
            }

            public IResponseResolver<TResult> Decompress(Func<byte[], byte[]> decompressor)
            {
                innerResolver.Decompress(decompressor);
                return this;
            }

            public IResponseResolver<TResult> Deserialize(Func<byte[], TResult> deserializer)
            {
                innerResolver.Deserialize(deserializer);
                return this;
            }

            public Task<IResponse<TResult>> ResolveAsync()
            {
                return innerResolver.ResolveAsync<TResult>();
            }

            public async Task<IResponse<TNewResult>> ResolveAsync<TNewResult>(Func<TResult, TNewResult> transformer)
                where TNewResult : class
            {
                innerResolver.ResolveInternal();

                var readOnlyHeaders = new ReadOnlyDictionary<string, string>(innerResolver.Headers);

                if (!innerResolver.IsSuccess)
                {
                    var error = await innerResolver.HttpResponseMessage.Content.ReadAsAsync<Error>();
                    return new HttpResponse<TNewResult>(innerResolver.HttpResponseMessage, innerResolver.IsSuccess,
                        innerResolver.StatusCode, innerResolver.RequestId, readOnlyHeaders, error);
                }

                var result = await innerResolver.ResolveResultAsync<TResult>();
                var newResult = transformer(result);

                return new HttpResponse<TNewResult>(innerResolver.HttpResponseMessage, innerResolver.IsSuccess,
                    innerResolver.StatusCode, innerResolver.RequestId, readOnlyHeaders, newResult);
            }
        }

        #region Decompress

        private byte[] AutoDecompressContent(byte[] data)
        {
            // Try decompress data if necessary
            if (TryGetCompressTypeHeader(out var compressType))
            {
                var optionalBodyRawSize = GetOptionalBodyRawSizeHeader();
                // Replace the data
                return DecompressContent(compressType, data, optionalBodyRawSize);
            }

            return data;
        }

        private bool TryGetCompressTypeHeader(out CompressType compressType)
        {
            if (!HttpResponseMessage.Headers.TryGetValues(LogHeaders.CompressType, out var compressTypes))
            {
                // No header
                compressType = CompressType.None;
                return false;
            }

            var compressTypeValue =
                compressTypes.FirstOrDefault(); // Fault tolerance (TODO: Show warns about duplicated keys)
            if (compressTypeValue.IsEmpty())
            {
                // Header is empty
                compressType = CompressType.None;
                return false;
            }

            // Convert value to enum
            return Enum.TryParse(compressTypeValue, true, out compressType)
                ? true
                : throw new ArgumentException($"Compress type [{compressTypeValue}] is not supported.",
                    LogHeaders.CompressType);
        }

        private int? GetOptionalBodyRawSizeHeader()
        {
            if (!HttpResponseMessage.Headers.TryGetValues(LogHeaders.BodyRawSize, out var bodyRawSizes))
                // No header
                return null;

            var bodyRawSizeValue =
                bodyRawSizes.FirstOrDefault(); // Fault tolerance (TODO: Show warns about duplicated keys)
            if (bodyRawSizeValue.IsEmpty())
                // Header is empty
                return null;

            return int.Parse(bodyRawSizeValue); // Let exception raise when format is incorrect.
        }

        private static byte[] DecompressContent(CompressType compressType, byte[] orignData, int? rawSize)
        {
            switch (compressType)
            {
                case CompressType.None:
                {
                    return orignData;
                }

                case CompressType.Lz4:
                {
                    if (!rawSize.HasValue)
                        throw new ArgumentException($"{LogHeaders.BodyRawSize} is required when using [lz4] compress.");

                    var rawData = LZ4Codec.Decode(orignData, 0, orignData.Length, rawSize.Value);
                    return rawData;
                }

                case CompressType.Deflate:
                {
                    var rawData = ZlibStream.UncompressBuffer(orignData);
                    return rawData;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(compressType), compressType, null);
                }
            }
        }

        #endregion Decompress
    }
}