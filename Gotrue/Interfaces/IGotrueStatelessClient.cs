using System;
using System.Threading.Tasks;
using static Supabase.Gotrue.Constants;
using static Supabase.Gotrue.StatelessClient;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueStatelessClient<TUser, TSession>
        where TUser : User
        where TSession : Session
    {
        Task<TUser?> CreateUser(string jwt, StatelessClientOptions options, AdminUserAttributes attributes);
        Task<TUser?> CreateUser(string jwt, StatelessClientOptions options, string email, string password, AdminUserAttributes? attributes = null);
        Task<bool> DeleteUser(string uid, string jwt, StatelessClientOptions options);
        IGotrueApi<TUser, TSession> GetApi(StatelessClientOptions options);
        Task<TSession?> GetSessionFromUrl(Uri uri, StatelessClientOptions options);
        Task<TUser?> GetUser(string jwt, StatelessClientOptions options);
        Task<TUser?> GetUserById(string jwt, StatelessClientOptions options, string userId);
        Task<bool> InviteUserByEmail(string email, string jwt, StatelessClientOptions options);
        Task<UserList<User>?> ListUsers(string jwt, StatelessClientOptions options, string? filter = null, string? sortBy = null, SortOrder sortOrder = SortOrder.Descending, int? page = null, int? perPage = null);
        Task<TSession?> RefreshToken(string refreshToken, StatelessClientOptions options);
        Task<bool> ResetPasswordForEmail(string email, StatelessClientOptions options);
        Task<bool> SendMagicLink(string email, StatelessClientOptions options, SignInOptions? signInOptions = null);
        ProviderAuthState SignIn(Provider provider, StatelessClientOptions options, SignInOptions? signInOptions = null);
        Task<TSession?> SignIn(SignInType type, string identifierOrToken, string? password = null, StatelessClientOptions? options = null);
        Task<bool> SignIn(string email, StatelessClientOptions options, SignInOptions? signInOptions = null);
        Task<TSession?> SignIn(string email, string password, StatelessClientOptions options);
        Task<bool> SignOut(string jwt, StatelessClientOptions options);
        Task<TSession?> SignUp(SignUpType type, string identifier, string password, StatelessClientOptions options, SignUpOptions? signUpOptions = null);
        Task<TSession?> SignUp(string email, string password, StatelessClientOptions options, SignUpOptions? signUpOptions = null);
        Task<TUser?> Update(string accessToken, UserAttributes attributes, StatelessClientOptions options);
        Task<TUser?> UpdateUserById(string jwt, StatelessClientOptions options, string userId, AdminUserAttributes userData);
        Task<TSession?> VerifyOTP(string phone, string token, StatelessClientOptions options, MobileOtpType type = MobileOtpType.SMS);
        Task<TSession?> VerifyOTP(string email, string token, StatelessClientOptions options, EmailOtpType type = EmailOtpType.MagicLink);
    }
}