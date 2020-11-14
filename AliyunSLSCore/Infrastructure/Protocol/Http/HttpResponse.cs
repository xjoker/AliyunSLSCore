//
// HttpResponse.cs
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    /// <summary>
    ///     服务响应包装对象，包含未反序列化的原始数据，可通过 <c>ReadXxxAsync()</c> 方法读取原始报文。
    /// </summary>
    public class HttpResponse : IResponse
    {
        public HttpResponse(HttpResponseMessage responseMessage, bool isSuccess, HttpStatusCode statusCode,
            string requestId, IDictionary<string, string> headers, Error error)
        {
            ResponseMessage = responseMessage;
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            RequestId = requestId;
            Headers = headers;
            Error = error;
        }

        internal HttpResponseMessage ResponseMessage { get; }

        public HttpStatusCode StatusCode { get; }

        public bool IsSuccess { get; }

        public string RequestId { get; }

        public IDictionary<string, string> Headers { get; }

        public Error Error { get; }

        public IResponse EnsureSuccess()
        {
            if (!IsSuccess)
                throw Error == null
                    ? new LogServiceException(RequestId, ErrorCode.SdkInternalError,
                        "The error detail result is missing.")
                    : new LogServiceException(RequestId, Error.ErrorCode, Error.ErrorMessage);

            return this;
        }

        public Task<TResult> ReadAsAsync<TResult>()
        {
            return ResponseMessage.Content.ReadAsAsync<TResult>();
        }

        public Task<Stream> ReadAsByteStreamAsync()
        {
            return ResponseMessage.Content.ReadAsStreamAsync();
        }

        public Task<byte[]> ReadAsByteArrayAsync()
        {
            return ResponseMessage.Content.ReadAsByteArrayAsync();
        }

        public override string ToString()
        {
            return $"[{RequestId}] {StatusCode}{(IsSuccess ? string.Empty : " Error:" + Error)}";
        }
    }

    /// <summary>
    ///     服务响应包装对象，此类型包含一个已反序列化为 <typeparamref name="TResult" /> 的 <see cref="Result">结果对象</see>。
    /// </summary>
    /// <typeparam name="TResult">响应包含结果的类型。</typeparam>
    public class HttpResponse<TResult> : HttpResponse, IResponse<TResult>
        where TResult : class
    {
        public HttpResponse(HttpResponseMessage responseMessage, bool isSuccess, HttpStatusCode statusCode,
            string requestId, IDictionary<string, string> headers, Error error)
            : base(responseMessage, isSuccess, statusCode, requestId, headers, error)
        {
            Result = null;
        }

        public HttpResponse(HttpResponseMessage responseMessage, bool isSuccess, HttpStatusCode statusCode,
            string requestId, IDictionary<string, string> headers, TResult result)
            : base(responseMessage, isSuccess, statusCode, requestId, headers, null)
        {
            Result = result;
        }

        public TResult Result { get; }

        IResponse<TResult> IResponse<TResult>.EnsureSuccess()
        {
            EnsureSuccess();
            return this;
        }

        public override string ToString()
        {
            return base.ToString() + $" Result:{(Result == null ? "<null>" : Result.GetType().FullName)}";
        }
    }
}