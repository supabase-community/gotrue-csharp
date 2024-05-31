﻿using System;
using System.Threading.Tasks;
using Supabase.Core.Interfaces;
using Supabase.Gotrue.Responses;
using static Supabase.Gotrue.Constants;

#pragma warning disable CS1591

namespace Supabase.Gotrue.Interfaces
{
	public interface IGotrueApi<TUser, TSession> : IGettableHeaders
		where TUser : User
		where TSession : Session
	{
		Task<TUser?> CreateUser(string jwt, AdminUserAttributes? attributes = null);
		Task<BaseResponse> DeleteUser(string uid, string jwt);
		Task<TUser?> GetUser(string jwt);
		Task<TUser?> GetUserById(string jwt, string userId);
		Task<BaseResponse> InviteUserByEmail(string email, string jwt, InviteUserByEmailOptions? options = null);
		Task<UserList<TUser>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null, int? perPage = null);
		Task<TSession?> RefreshAccessToken(string accessToken, string refreshToken);
		Task<BaseResponse> ResetPasswordForEmail(string email);
		Task<ResetPasswordForEmailState> ResetPasswordForEmail(ResetPasswordForEmailOptions options);
		Task<BaseResponse> SendMagicLinkEmail(string email, SignInOptions? options = null);
		Task<BaseResponse> SendMobileOTP(string phone);
		Task<TSession?> SignInWithIdToken(Provider provider, string idToken, string? accessToken = null, string? nonce = null, string? captchaToken = null);
		Task<TSession?> SignInWithEmail(string email, string password);
		Task<TSession?> SignInWithPhone(string phone, string password);
		Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessEmailOptions options);
		Task<PasswordlessSignInState> SignInWithOtp(SignInWithPasswordlessPhoneOptions options);
		Task<TSession?> SignInAnonymously(SignInAnonymouslyOptions? options = null);
		Task<SsoResponse?> SignInWithSso(Guid providerId, SignInOptionsWithSsoOptions? options = null);
		Task<SsoResponse?> SignInWithSso(string domain, SignInOptionsWithSsoOptions? options = null);
		Task<BaseResponse> SignOut(string jwt);
		Task<TSession?> SignUpWithEmail(string email, string password, SignUpOptions? options = null);
		Task<TSession?> SignUpWithPhone(string phone, string password, SignUpOptions? options = null);
		Task<TUser?> UpdateUser(string jwt, UserAttributes attributes);
		Task<TUser?> UpdateUserById(string jwt, string userId, UserAttributes userData);
		Task<TSession?> VerifyMobileOTP(string phone, string token, MobileOtpType type);
		Task<TSession?> VerifyEmailOTP(string email, string token, EmailOtpType type);
		Task<BaseResponse> Reauthenticate(string userJwt);
		ProviderAuthState GetUriForProvider(Provider provider, SignInOptions? options = null);
		Task<Session?> ExchangeCodeForSession(string codeVerifier, string authCode);
		Task<Settings?> Settings();
		Task<BaseResponse> GenerateLink(string jwt, GenerateLinkOptions options);

		/// <summary>
		/// Links an oauth identity to an existing user.
		///
		/// This method requires the PKCE flow.
		/// </summary>
		/// <param name="token">User's token</param>
		/// <param name="provider">Provider to Link</param>
		/// <param name="options"></param>
		/// <returns></returns>
		Task<ProviderAuthState> LinkIdentity(string token, Provider provider, SignInOptions options);

		/// <summary>
		/// Unlinks an identity from a user by deleting it. The user will no longer be able to sign in with that identity once it's unlinked.
		/// </summary>
		/// <param name="token">User's token</param>
		/// <param name="userIdentity">Identity to be unlinked</param>
		/// <returns></returns>
		Task<bool> UnlinkIdentity(string token, UserIdentity userIdentity);
	}

}
