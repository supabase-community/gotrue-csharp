using System;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueClient
    {
        ISession CurrentSession { get; }
        IUser CurrentUser { get; }

        event EventHandler<IClientStateChanged> StateChanged;

        Task<IUser> CreateUser(string jwt, IAdminUserAttributes attributes);
        Task<IUser> CreateUser(string jwt, string email, string password, IAdminUserAttributes attributes = null);
        Task<bool> DeleteUser(string uid, string jwt);
        Task<ISession> GetSessionFromUrl(Uri uri, bool storeSession = true);
        Task<IUser> GetUserById(string jwt, string userId);
        Task<bool> InviteUserByEmail(string email, string jwt);
        Task<IUserList> ListUsers(string jwt, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null);
        Task<ISession> RefreshSession();
        Task<bool> ResetPasswordForEmail(string email);
        Task<ISession> RetrieveSessionAsync();
        Task<bool> SendMagicLink(string email, ISignInOptions options = null);
        Task<string> SignIn(Client.Provider provider, string scopes = null);
        Task<ISession> SignIn(Client.SignInType type, string identifierOrToken, string password = null, string scopes = null);
        Task<bool> SignIn(string email, ISignInOptions options = null);
        Task<ISession> SignIn(string email, string password);
        Task SignOut();
        Task<ISession> SignUp(Client.SignUpType type, string identifier, string password, ISignUpOptions options = null);
        Task<ISession> SignUp(string email, string password, ISignUpOptions options = null);
        Task<IUser> Update(IUserAttributes attributes);
        Task<IUser> UpdateUserById(string jwt, string userId, IAdminUserAttributes userData);
        Task<ISession> VerifyOTP(string phone, string token);
    }
}