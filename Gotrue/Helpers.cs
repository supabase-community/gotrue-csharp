﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Responses;

namespace Supabase.Gotrue
{
	/// <summary>
	/// Utility methods to assist with flow. Includes nonce generation & verification.
	/// </summary>
	public static class Helpers
	{
		/// <summary>
		/// Generates a nonce (code verifier)
		/// Used with PKCE flow and Apple/Google Sign in.
		/// Paired with <see cref="GeneratePKCENonceVerifier(string)"/>
		///
		/// Sourced from: https://stackoverflow.com/a/65220376/3629438
		/// </summary>
		public static string GenerateNonce()
		{
			// ReSharper disable once StringLiteralTypo
			const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
			var random = new Random();
			var nonce = new char[128];
			for (var i = 0; i < nonce.Length; i++)
			{
				nonce[i] = chars[random.Next(chars.Length)];
			}

			return new string(nonce);
		}

		/// <summary>
		/// Generates a PKCE SHA256 code challenge given a nonce (code verifier)
		/// 
		/// Paired with <see cref="GenerateNonce"/>
		///
		/// Sourced from: https://stackoverflow.com/a/65220376/3629438
		/// </summary>
		/// <param name="codeVerifier"></param>
		public static string GeneratePKCENonceVerifier(string codeVerifier)
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
		/// Generates a SHA256 nonce given a rawNonce, used Apple/Google Sign in.
		/// </summary>
		/// <param name="rawNonce"></param>
		/// <returns></returns>
		public static string GenerateSHA256NonceFromRawNonce(string rawNonce)
		{
			var sha = new SHA256Managed();
			var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
			var hash = sha.ComputeHash(utf8RawNonce);

			var result = string.Empty;
			foreach (var t in hash)
				result += t.ToString("x2");

			return result;
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

		private static readonly HttpClient Client = new HttpClient();

		/// <summary>
		/// Helper to make a request using the defined parameters to an API Endpoint and coerce into a model. 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="method"></param>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <param name="headers"></param>
		/// <returns></returns>
		internal static async Task<T?> MakeRequest<T>(HttpMethod method, string url, object? data = null, Dictionary<string, string>? headers = null)
			where T : class
		{
			var baseResponse = await MakeRequest(method, url, data, headers);
			return baseResponse.Content != null ? JsonConvert.DeserializeObject<T>(baseResponse.Content) : default;
		}

		/// <summary>
		/// Helper to make a request using the defined parameters to an API Endpoint.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="url"></param>
		/// <param name="data"></param>
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

			using var requestMessage = new HttpRequestMessage(method, builder.Uri);
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

			var response = await Client.SendAsync(requestMessage);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				var e = new GotrueException(content ?? "Request Failed")
				{
					Content = content,
					Response = response,
					StatusCode = (int)response.StatusCode
				};
				e.AddReason();
				throw e;
			}

			return new BaseResponse { Content = content, ResponseMessage = response };

		}
	}
}
