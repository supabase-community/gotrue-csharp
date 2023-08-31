using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GotrueTests
{
	public static class TestUtils
	{
		private static readonly Random Random = new Random();

		public const string PASSWORD = "I@M@SuperP@ssWord";
		
		public static void LogDebug(string message, Exception e)
		{
			Debug.WriteLine(message);
			if (e != null)
				Debug.WriteLine(e);
		}


		public static string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		public static string GetRandomPhoneNumber()
		{
			const string chars = "123456789";
			var inner = new string(Enumerable.Repeat(chars, 10).Select(s => s[Random.Next(s.Length)]).ToArray());
			return $"+1{inner}";
		}

		public static string GenerateServiceRoleToken()
		{
			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("37c304f8-51aa-419a-a1af-06154e63707a")); // using GOTRUE_JWT_SECRET

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				IssuedAt = DateTime.Now,
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
				Claims = new Dictionary<string, object>() { { "role", "service_role" } }
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var securityToken = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(securityToken);
		}

	}
}
