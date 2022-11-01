using System;
using System.Threading.Tasks;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueStatelessClient<TUser, TSession>
        where TUser : User
        where TSession : Session
    {
        Task<TUser> CreateUser(string jwt, StatelessClient.StatelessClientOptions options, AdminUserAttributes attributes);
        Task<TUser> CreateUser(string jwt, StatelessClient.StatelessClientOptions options, string email, string password, AdminUserAttributes attributes = null);
        Task<bool> DeleteUser(string uid, string jwt, StatelessClient.StatelessClientOptions options);
        IGotrueApi<TUser, TSession> GetApi(StatelessClient.StatelessClientOptions options);
        Task<TSession> GetSessionFromUrl(Uri uri, StatelessClient.StatelessClientOptions options);
        Task<TUser> GetUser(string jwt, StatelessClient.StatelessClientOptions options);
        Task<TUser> GetUserById(string jwt, StatelessClient.StatelessClientOptions options, string userId);
        Task<bool> InviteUserByEmail(string email, string jwt, StatelessClient.StatelessClientOptions options);
        Task<UserList<User>> ListUsers(string jwt, StatelessClient.StatelessClientOptions options, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null);
        Task<TSession> RefreshToken(string refreshToken, StatelessClient.StatelessClientOptions options);
        Task<bool> ResetPasswordForEmail(string email, StatelessClient.StatelessClientOptions options);
        Task<bool> SendMagicLink(string email, StatelessClient.StatelessClientOptions options, SignInOptions signInOptions = null);
        string SignIn(Constants.Provider provider, StatelessClient.StatelessClientOptions options, string scopes = null);
        Task<TSession> SignIn(Constants.SignInType type, string identifierOrToken, string password = null, StatelessClient.StatelessClientOptions options = null);
        Task<bool> SignIn(string email, StatelessClient.StatelessClientOptions options, SignInOptions signInOptions = null);
        Task<TSession> SignIn(string email, string password, StatelessClient.StatelessClientOptions options);
        Task<bool> SignOut(string jwt, StatelessClient.StatelessClientOptions options);
        Task<TSession> SignUp(Constants.SignUpType type, string identifier, string password, StatelessClient.StatelessClientOptions options, SignUpOptions signUpOptions = null);
        Task<TSession> SignUp(string email, string password, StatelessClient.StatelessClientOptions options, SignUpOptions signUpOptions = null);
        Task<TUser> Update(string accessToken, UserAttributes attributes, StatelessClient.StatelessClientOptions options);
        Task<TUser> UpdateUserById(string jwt, StatelessClient.StatelessClientOptions options, string userId, AdminUserAttributes userData);
        Task<TSession> VerifyOTP(string phone, string token, StatelessClient.StatelessClientOptions options, MobileOtpType type = MobileOtpType.SMS);
        Task<TSession> VerifyOTP(string email, string token, StatelessClient.StatelessClientOptions options, EmailOtpType type = EmailOtpType.MagicLink);
    }
}