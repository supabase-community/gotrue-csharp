using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Supabase.Gotrue.Responses;
using System.Threading;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Supabase.Gotrue
{
    public static class Helpers
    {
        /// <summary>
        /// Generates a nonce (code verifier)
        /// Used with PKCE flow and Apple/Google Sign in.
        /// Paired with <see cref="GenerateNonceVerifier(string)"/>
        ///
        /// Sourced from: https://stackoverflow.com/a/65220376/3629438
        /// </summary>
        public static string GenerateNonce()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
            var random = new Random();
            var nonce = new char[128];
            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] = chars[random.Next(chars.Length)];
            }

            return new string(nonce);
        }

        /// <summary>
        /// Generates a SHA256 code challenge given a nonce (code verifier)
        /// Used with PKCE float and Apple/Google Sign in.
        /// Paired with <see cref="GenerateNonce"/>
        ///
        /// Sourced from: https://stackoverflow.com/a/65220376/3629438
        /// </summary>
        /// <param name="codeVerifier"></param>
        public static string GenerateNonceVerifier(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var b64Hash = Convert.ToBase64String(hash);
            var code = Regex.Replace(b64Hash, "\\+", "-");
            code = Regex.Replace(code, "\\/", "_");
            code = Regex.Replace(code, "=+$", "");
            return code;
        }

        /// <summary>
        /// Adds query params to a given Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static Uri AddQueryParams(string url, Dictionary<string, string> data)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var param in data)
                query[param.Key] = param.Value;

            builder.Query = query.ToString();

            return builder.Uri;
        }

        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Helper to make a request using the defined parameters to an API Endpoint and coerce into a model. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="reqParams"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        internal static async Task<T?> MakeRequest<T>(HttpMethod method, string url, object? data = null, Dictionary<string, string>? headers = null) where T : class
        {
            var baseResponse = await MakeRequest(method, url, data, headers);
            return baseResponse.Content != null ? JsonConvert.DeserializeObject<T>(baseResponse.Content) : default;
        }

        /// <summary>
        /// Helper to make a request using the defined parameters to an API Endpoint.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="reqParams"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        internal static async Task<BaseResponse> MakeRequest(HttpMethod method, string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            if (data != null && method == HttpMethod.Get)
            {
                // Case if it's a Get request the data object is a dictionary<string,string>
                if (data is Dictionary<string, string> reqParams)
                {
                    foreach (var param in reqParams)
                        query[param.Key] = param.Value;
                }

            }

            builder.Query = query.ToString();

            using (var requestMessage = new HttpRequestMessage(method, builder.Uri))
            {

                if (data != null && method != HttpMethod.Get)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                }

                if (headers != null)
                {
                    foreach (var kvp in headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                    }
                }

                var response = await client.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var obj = new ErrorResponse
                    {
                        Content = content,
                        Message = content
                    };
                    throw new RequestException(response, obj);
                }
                else
                {
                    return new BaseResponse { Content = content, ResponseMessage = response };
                }
            }
        }
    }
}
