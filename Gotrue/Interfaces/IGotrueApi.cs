using Supabase.Core.Interfaces;
using Supabase.Gotrue.Responses;
using System.Threading.Tasks;
using static Supabase.Gotrue.Constants;

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
        Task<BaseResponse> InviteUserByEmail(string email, string jwt);
        Task<UserList<TUser>?> ListUsers(string jwt, string? filter = null, string? sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null);
        Task<TSession?> RefreshAccessToken(string refreshToken);
        Task<BaseResponse> ResetPasswordForEmail(string email);
        Task<BaseResponse> SendMagicLinkEmail(string email, SignInOptions? options = null);
        Task<BaseResponse> SendMobileOTP(string phone);
        Task<TSession?> SignInWithEmail(string email, string password);
        Task<TSession?> SignInWithPhone(string phone, string password);
        Task<BaseResponse> SignOut(string jwt);
        Task<TSession?> SignUpWithEmail(string email, string password, SignUpOptions? options = null);
        Task<TSession?> SignUpWithPhone(string phone, string password, SignUpOptions? options = null);
        Task<TUser?> UpdateUser(string jwt, UserAttributes attributes);
        Task<TUser?> UpdateUserById(string jwt, string userId, UserAttributes userData);
        Task<TSession?> VerifyMobileOTP(string phone, string token, MobileOtpType type);
        Task<TSession?> VerifyEmailOTP(string email, string token, EmailOtpType type);

        ProviderUri GetUriForProvider(Provider provider, SignInOptions? options = null);
        Task<Session?> ExchangeCodeForSession(string codeVerifier, string authCode);
    }
}