using Supabase.Gotrue.Interfaces;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueApi
    {
        Task<IUser> CreateUser(string jwt, IAdminUserAttributes attributes = null);
        Task<T> CreateUser<T>(string jwt, IAdminUserAttributes attributes = null) where T : IUser;
        Task<IBaseResponse> DeleteUser(string uid, string jwt);
        Task<T> DeleteUser<T>(string uid, string jwt) where T : IBaseResponse;
        string GetUrlForProvider(Client.Provider provider, string scopes = null);
        Task<IUser> GetUser(string jwt);
        Task<T> GetUser<T>(string jwt) where T : IUser;
        Task<IUser> GetUserById(string jwt, string userId);
        Task<T> GetUserById<T>(string jwt, string userId) where T : IUser;
        Task<IBaseResponse> InviteUserByEmail(string email, string jwt);
        Task<T> InviteUserByEmail<T>(string email, string jwt) where T : IBaseResponse;
        Task<IUserList> ListUsers(string jwt, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null);
        Task<T> ListUsers<T>(string jwt, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null) where T : IUserList;
        Task<ISession> RefreshAccessToken(string refreshToken);
        Task<T> RefreshAccessToken<T>(string refreshToken) where T : ISession;
        Task<IBaseResponse> ResetPasswordForEmail(string email);
        Task<T> ResetPasswordForEmail<T>(string email) where T : IBaseResponse;
        Task<IBaseResponse> SendMagicLinkEmail(string email, ISignInOptions options = null);
        Task<T> SendMagicLinkEmail<T>(string email, ISignInOptions options = null) where T : IBaseResponse;
        Task<IBaseResponse> SendMobileOTP(string phone);
        Task<T> SendMobileOTP<T>(string phone) where T : IBaseResponse;
        Task<ISession> SignInWithEmail(string email, string password);
        Task<T> SignInWithEmail<T>(string email, string password) where T : ISession;
        Task<ISession> SignInWithPhone(string phone, string password);
        Task<T> SignInWithPhone<T>(string phone, string password) where T : ISession;
        Task<IBaseResponse> SignOut(string jwt);
        Task<T> SignOut<T>(string jwt) where T : IBaseResponse;
        Task<ISession> SignUpWithEmail(string email, string password, ISignUpOptions options = null);
        Task<T> SignUpWithEmail<T>(string email, string password, ISignUpOptions options = null) where T : ISession;
        Task<ISession> SignUpWithPhone(string phone, string password, ISignUpOptions options = null);
        Task<T> SignUpWithPhone<T>(string phone, string password, ISignUpOptions options = null) where T : ISession;
        Task<IUser> UpdateUser(string jwt, IUserAttributes attributes);
        Task<T> UpdateUser<T>(string jwt, IUserAttributes attributes) where T : IUser;
        Task<IUser> UpdateUserById(string jwt, string userId, IUserAttributes userData);
        Task<T> UpdateUserById<T>(string jwt, string userId, IUserAttributes userData) where T : IUser;
        Task<ISession> VerifyMobileOTP(string phone, string token);
        Task<T> VerifyMobileOTP<T>(string phone, string token) where T : ISession;
    }
}