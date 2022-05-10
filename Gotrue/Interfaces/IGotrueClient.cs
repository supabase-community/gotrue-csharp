using Supabase.Gotrue.Interfaces;
using System;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueClient
    {
        /// <summary>
        /// The current Session
        /// </summary>
        /// <value>
        /// The current session.
        /// </value>
        ISession CurrentSession { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        IUser CurrentUser { get; }

        /// <summary>
        /// Occurs when [state changed].
        /// </summary>
        event EventHandler<IClientStateChanged> StateChanged;

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<IUser> CreateUser(string jwt, IAdminUserAttributes attributes);

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<IUser> CreateUser(string jwt, string email, string password, IAdminUserAttributes attributes = null);

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<T> CreateUser<T>(string jwt, IAdminUserAttributes attributes) where T : IUser;

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<T> CreateUser<T>(string jwt, string email, string password, IAdminUserAttributes attributes = null) where T : IUser;
        
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<bool> DeleteUser(string uid, string jwt);
       
        /// <summary>
        /// Gets the session from URL.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="storeSession">if set to <c>true</c> [store session].</param>
        /// <returns></returns>
        Task<ISession> GetSessionFromUrl(Uri uri, bool storeSession = true);
        
        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        Task<IUser> GetUserById(string jwt, string userId);
        
        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        Task<T> GetUserById<T>(string jwt, string userId) where T : IUser;
        
        /// <summary>
        /// Invites the user by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<bool> InviteUserByEmail(string email, string jwt);
        
        /// <summary>
        /// Lists the users.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The per page.</param>
        /// <returns></returns>
        Task<IUserList> ListUsers(string jwt, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null);
        
        /// <summary>
        /// Lists the users.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The per page.</param>
        /// <returns></returns>
        Task<T> ListUsers<T>(string jwt, string filter = null, string sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null) where T : IUserList;
        
        /// <summary>
        /// Refreshes the session.
        /// </summary>
        /// <returns></returns>
        Task<ISession> RefreshSession();
        
        /// <summary>
        /// Refreshes the session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> RefreshSession<T>() where T : ISession;
        
        /// <summary>
        /// Resets the password for email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        Task<bool> ResetPasswordForEmail(string email);
        
        /// <summary>
        /// Retrieves the session asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<ISession> RetrieveSessionAsync();
        
        /// <summary>
        /// Sends the magic link.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<bool> SendMagicLink(string email, ISignInOptions options = null);
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        Task<string> SignIn(Client.Provider provider, string scopes = null);
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="identifierOrToken">The identifier or token.</param>
        /// <param name="password">The password.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        Task<ISession> SignIn(Client.SignInType type, string identifierOrToken, string password = null, string scopes = null);
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<bool> SignIn(string email, ISignInOptions options = null);
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<ISession> SignIn(string email, string password);
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="identifierOrToken">The identifier or token.</param>
        /// <param name="password">The password.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        Task<T> SignIn<T>(Client.SignInType type, string identifierOrToken, string password = null, string scopes = null) where T : ISession;
        
        /// <summary>
        /// Signs the in.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<T> SignIn<T>(string email, string password) where T : ISession;
        
        /// <summary>
        /// Represents an event that is raised when the sign-out operation is complete.
        /// </summary>
        /// <returns></returns>
        Task SignOut();
        
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<ISession> SignUp(Client.SignUpType type, string identifier, string password, ISignUpOptions options = null);
        
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<ISession> SignUp(string email, string password, ISignUpOptions options = null);
        
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<T> SignUp<T>(Client.SignUpType type, string identifier, string password, ISignUpOptions options = null) where T : ISession;
        
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<T> SignUp<T>(string email, string password, ISignUpOptions options = null) where T : ISession;
        
        /// <summary>
        /// Updates the specified attributes.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<IUser> Update(IUserAttributes attributes);
        
        /// <summary>
        /// Updates the specified attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<T> Update<T>(IUserAttributes attributes) where T : IUser;
        
        /// <summary>
        /// Updates the user by identifier.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        Task<IUser> UpdateUserById(string jwt, string userId, IAdminUserAttributes userData);
        
        /// <summary>
        /// Updates the user by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        Task<T> UpdateUserById<T>(string jwt, string userId, IAdminUserAttributes userData) where T : IUser;
        
        /// <summary>
        /// Verifies the otp.
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<ISession> VerifyOTP(string phone, string token);
        
        /// <summary>
        /// Verifies the otp.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phone">The phone.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<T> VerifyOTP<T>(string phone, string token) where T : ISession;
    }
}