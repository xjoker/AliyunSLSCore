//
// HttpRequestMessageBuilder.cs
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Aliyun.Api.LogService.Infrastructure.Authentication;
using Aliyun.Api.LogService.Utils;
using Google.Protobuf;
using Ionic.Zlib;
using LZ4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    /// <summary>
    ///     Builder for constructing the <see cref="HttpRequestMessage" />.
    /// </summary>
    /// <inheritdoc />
    public class HttpRequestMessageBuilder : IRequestBuilder<HttpRequestMessage>
    {
        private static readonly byte[] EmptyByteArray = new byte[0];

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly Encoding encoding;

        private readonly HttpRequestMessage httpRequestMessage;

        private readonly string path;

        private readonly IDictionary<string, string> query;

        /// <summary>
        ///     The real content to transfer.
        /// </summary>
        private object content;

        /// <summary>
        ///     Proceed the actions after content prepared (i.e., all transforms (e.g., serialize, compress, encrypt, encode) of
        ///     <see cref="content" /> are applied).
        /// </summary>
        private Action contentHandler;

        /// <summary>
        ///     The Content-MD5 header in HEX format.
        /// </summary>
        private string contentMd5Hex;

        /// <summary>
        ///     The authentication credential.
        /// </summary>
        private Credential credential;

        /// <summary>
        ///     The signature type.
        /// </summary>
        private SignatureType signatureType;

        public HttpRequestMessageBuilder(HttpMethod method, string uri)
        {
            httpRequestMessage = new HttpRequestMessage(method, uri);
            encoding = Encoding.UTF8;
            ParseUri(uri, out path, out query);

            FillDefaultHeaders();
        }

        /// <summary>
        ///     Gets the serialized content.
        /// </summary>
        private byte[] SerializedContent =>
            content == null
                ? null
                : content as byte[]
                  ?? throw new InvalidOperationException("Content must serialized before this operation.");

        #region Serialize

        public IRequestBuilder<HttpRequestMessage> Serialize(SerializeType serializeType)
        {
            switch (content)
            {
                case null:
                    throw new InvalidOperationException("Nothing to serialize.");
                case byte[] _:
                    throw new InvalidOperationException("Content has already been serialized.");
            }

            switch (serializeType)
            {
                case SerializeType.Json:
                {
                    ContentHeader(x => x.ContentType = new MediaTypeHeaderValue("application/json"));
                    var json = JsonConvert.SerializeObject(content, JsonSerializerSettings);
                    Content(encoding.GetBytes(json));

                    break;
                }

                case SerializeType.Protobuf:
                {
                    if (!(content is IMessage protoMessage))
                        throw new ArgumentException("Serialization of ProtoBuf requires IMessage.");

                    ContentHeader(x => x.ContentType = new MediaTypeHeaderValue("application/x-protobuf"));
                    Content(protoMessage.ToByteArray());

                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(serializeType), serializeType, null);
                }
            }

            return this;
        }

        #endregion Serialize

        #region Compress

        public IRequestBuilder<HttpRequestMessage> Compress(CompressType compressType)
        {
            if (SerializedContent == null) throw new InvalidOperationException("Nothing to compress.");

            switch (compressType)
            {
                case CompressType.None:
                {
                    break;
                }

                case CompressType.Lz4:
                {
                    SetCompressType("lz4");
                    content = LZ4Codec.Encode(SerializedContent, 0, SerializedContent.Length);
                    break;
                }

                case CompressType.Deflate:
                {
                    SetCompressType("deflate");
                    content = ZlibStream.CompressBuffer(SerializedContent);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(compressType), compressType, null);
                }
            }

            return this;
        }

        #endregion Compress

        #region Authentication

        public IRequestBuilder<HttpRequestMessage> Authenticate(Credential credential)
        {
            Ensure.NotNull(credential, nameof(credential));
            Ensure.NotEmpty(credential.AccessKeyId, nameof(credential.AccessKeyId));
            Ensure.NotEmpty(credential.AccessKey, nameof(credential.AccessKey));

            this.credential = credential;
            return this;
        }

        #endregion Authentication

        public HttpRequestMessage Build()
        {
            // Validate
            Ensure.NotNull(credential, nameof(credential));
            Ensure.NotEmpty(credential.AccessKeyId, nameof(credential.AccessKeyId));
            Ensure.NotEmpty(credential.AccessKey, nameof(credential.AccessKey));

            // Rebuild the RequestUri
            var queryString = string.Join("&", query
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}"));
            var pathAndQuery = queryString.IsNotEmpty() ? $"{path}?{queryString}" : path;
            httpRequestMessage.RequestUri = new Uri(pathAndQuery, UriKind.Relative);

            // Process sts-token.
            var hasSecurityToken =
                httpRequestMessage.Headers.TryGetValues(LogHeaders.SecurityToken, out var securityTokens)
                && securityTokens.FirstOrDefault().IsNotEmpty();

            if (!hasSecurityToken && credential.StsToken.IsNotEmpty())
                httpRequestMessage.Headers.Add(LogHeaders.SecurityToken, credential.StsToken);

            // NOTE: If x-log-bodyrawsize is empty, fill it with "0". Otherwise, some method call will be corrupted.
            if (!httpRequestMessage.Headers.Contains(LogHeaders.BodyRawSize)) SetBodyRawSize(0);

            // Build content if necessary
            if (SerializedContent.IsNotEmpty())
            {
                httpRequestMessage.Content = new ByteArrayContent(SerializedContent);
                contentHandler?.Invoke();

                // Prepare header
                ContentHeader(x =>
                {
                    // Compute actual length
                    x.ContentLength = SerializedContent.Length;
                    // Compute actual MD5
                    contentMd5Hex = BitConverter.ToString(CalculateContentMd5()).Replace("-", string.Empty);

                    x.Add("Content-MD5", contentMd5Hex); // Non-standard header
                });
            }
            else if (httpRequestMessage.Method == HttpMethod.Post || httpRequestMessage.Method == HttpMethod.Put)
            {
                // When content is empty as well as method is `POST` or `PUT`, generate an empty content and corresponding headers.

                httpRequestMessage.Content = new ByteArrayContent(EmptyByteArray);
                // Don't invoke `contentHandler` here!

                /*
                 * NOTE:
                 * Here is a annoying hack, the log service service cannot accept empty `Content-Type`
                 * header when POST or PUT methods. So, we have to force set some header value.
                 */
                ContentHeader(x =>
                {
                    x.ContentType = new MediaTypeHeaderValue("application/json");
                    // For some reason, I think it is better to set `Content-Type` to `0` to prevent
                    // some unexpected behavior on server side.
                    x.ContentLength = 0;
                });
            }

            // Do signature
            var signature = Convert.ToBase64String(ComputeSignature());
            httpRequestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("LOG", $"{credential.AccessKeyId}:{signature}");

            return httpRequestMessage;
        }

        private static void ParseUri(string uri, out string path, out IDictionary<string, string> query)
        {
            var absUri = new Uri(new Uri("http://fa.ke"), uri);
            path = absUri.AbsolutePath;
            query = absUri.ParseQueryString()
                .ToEnumerable()
                .ToDictionary(kv => kv.Key, kv => kv.Value); // NOTE: Restricted mode, key cannot be duplicated.
        }

        private void FillDefaultHeaders()
        {
            httpRequestMessage.Headers.Date = DateTimeOffset.Now;
            httpRequestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("log-dotnetcore-sdk",
                Constants.AssemblyVersion));
            httpRequestMessage.Headers.Add(LogHeaders.ApiVersion, "0.6.0");
        }

        #region Query

        public IRequestBuilder<HttpRequestMessage> Query(string key, string value)
        {
            query.Add(key, value);
            return this;
        }

        public IRequestBuilder<HttpRequestMessage> Query(object queryModel)
        {
            foreach (var kv in JObject.FromObject(queryModel, JsonSerializer.CreateDefault(JsonSerializerSettings)))
                query.Add(kv.Key, kv.Value.Value<string>());

            return this;
        }

        #endregion Query

        #region Header

        /// <summary>
        ///     Set headers of <see cref="T:System.Net.Http.Headers.HttpRequestHeaders" />
        /// </summary>
        /// <inheritdoc />
        public IRequestBuilder<HttpRequestMessage> Header(string key, string value)
        {
            httpRequestMessage.Headers.Add(key, value);

            return this;
        }

        private void ContentHeader(Action<HttpContentHeaders> option)
        {
            if (httpRequestMessage.Content == null)
                contentHandler += () => option(httpRequestMessage.Content.Headers);
            else
                option(httpRequestMessage.Content.Headers);
        }

        private void SetBodyRawSize(int size)
        {
            httpRequestMessage.Headers.Add(LogHeaders.BodyRawSize, size.ToString());
        }

        private void SetCompressType(string compressType)
        {
            httpRequestMessage.Headers.Add(LogHeaders.CompressType, compressType);
        }

        private void SetSignatureMethod(string signatureMethod)
        {
            httpRequestMessage.Headers.Add(LogHeaders.SignatureMethod, signatureMethod);
        }

        #endregion Header

        #region Content

        public IRequestBuilder<HttpRequestMessage> Content(byte[] content)
        {
            return Content((object) content);
        }

        public IRequestBuilder<HttpRequestMessage> Content(object content)
        {
            this.content = content;

            if (content is byte[] data) SetBodyRawSize(data.Length);

            return this;
        }

        #endregion Content

        #region Sign

        public IRequestBuilder<HttpRequestMessage> Sign(SignatureType signatureType)
        {
            this.signatureType = signatureType;
            return this;
        }

        private byte[] ComputeSignature()
        {
            switch (signatureType)
            {
                case SignatureType.HmacSha1:
                {
                    using (var hasher = new HMACSHA1(encoding.GetBytes(credential.AccessKey)))
                    {
                        SetSignatureMethod("hmac-sha1"); // This header must be set before generating sign source.
                        var signSource = GenerateSignSource();
                        var sign = hasher.ComputeHash(encoding.GetBytes(signSource));

                        return sign;
                    }
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(signatureType), signatureType,
                        "Currently only support [hmac-sha1] signature.");
                }
            }
        }

        private string GenerateSignSource()
        {
            var verb = httpRequestMessage.Method.Method;
            var contentMd5 = contentMd5Hex;
            var contentType = httpRequestMessage.Content?.Headers.ContentType.MediaType;
            var date = httpRequestMessage.Headers.Date?.ToString("r"); /* RFC 822 format */
            var logHeaders = string.Join("\n", httpRequestMessage.Headers
                .Concat(httpRequestMessage.Content?.Headers ??
                        Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .Where(x => x.Key.StartsWith("x-log") || x.Key.StartsWith("x-acs"))
                .Select(x =>
                    new KeyValuePair<string, string>(x.Key.ToLower(), x.Value.SingleOrDefault() /* Fault tolerance */))
                .Where(x => x.Value.IsNotEmpty()) // Remove empty header
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}:{x.Value}"));

            var resource = httpRequestMessage.RequestUri.OriginalString;

            return string.Join("\n", verb, contentMd5 ?? string.Empty, contentType ?? string.Empty, date, logHeaders,
                resource);
        }

        private byte[] CalculateContentMd5()
        {
            using (var hasher = MD5.Create())
            {
                return hasher.ComputeHash(SerializedContent);
            }
        }

        #endregion Sign
    }
}