using Supabase.Gotrue.Interfaces;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueApi
    {
        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<IUser> CreateUser(string jwt, IAdminUserAttributes attributes = null);
        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<T> CreateUser<T>(string jwt, IAdminUserAttributes attributes = null) where T : IUser;
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<IBaseResponse> DeleteUser(string uid, string jwt);
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uid">The uid.</param>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<T> DeleteUser<T>(string uid, string jwt) where T : IBaseResponse;
        /// <summary>
        /// Gets the URL for provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="scopes">The scopes.</param>
        /// <returns></returns>
        string GetUrlForProvider(Client.Provider provider, string scopes = null);
        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<IUser> GetUser(string jwt);
        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<T> GetUser<T>(string jwt) where T : IUser;
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
        Task<IBaseResponse> InviteUserByEmail(string email, string jwt);
        /// <summary>
        /// Invites the user by email.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<T> InviteUserByEmail<T>(string email, string jwt) where T : IBaseResponse;
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
        /// Refreshes the access token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        Task<ISession> RefreshAccessToken(string refreshToken);
        /// <summary>
        /// Refreshes the access token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        Task<T> RefreshAccessToken<T>(string refreshToken) where T : ISession;
        /// <summary>
        /// Resets the password for email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        Task<IBaseResponse> ResetPasswordForEmail(string email);
        /// <summary>
        /// Resets the password for email.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        Task<T> ResetPasswordForEmail<T>(string email) where T : IBaseResponse;
        /// <summary>
        /// Sends the magic link email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<IBaseResponse> SendMagicLinkEmail(string email, ISignInOptions options = null);
        /// <summary>
        /// Sends the magic link email.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<T> SendMagicLinkEmail<T>(string email, ISignInOptions options = null) where T : IBaseResponse;
        /// <summary>
        /// Sends the mobile otp.
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <returns></returns>
        Task<IBaseResponse> SendMobileOTP(string phone);
        /// <summary>
        /// Sends the mobile otp.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phone">The phone.</param>
        /// <returns></returns>
        Task<T> SendMobileOTP<T>(string phone) where T : IBaseResponse;
        /// <summary>
        /// Signs the in with email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<ISession> SignInWithEmail(string email, string password);
        /// <summary>
        /// Signs the in with email.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<T> SignInWithEmail<T>(string email, string password) where T : ISession;
        /// <summary>
        /// Signs the in with phone.
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<ISession> SignInWithPhone(string phone, string password);
        /// <summary>
        /// Signs the in with phone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phone">The phone.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        Task<T> SignInWithPhone<T>(string phone, string password) where T : ISession;
        /// <summary>
        /// Represents an event that is raised when the sign-out operation is complete.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<IBaseResponse> SignOut(string jwt);
        /// <summary>
        /// Represents an event that is raised when the sign-out operation is complete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <returns></returns>
        Task<T> SignOut<T>(string jwt) where T : IBaseResponse;
        /// <summary>
        /// Signs up with email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<ISession> SignUpWithEmail(string email, string password, ISignUpOptions options = null);
        /// <summary>
        /// Signs up with email.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<T> SignUpWithEmail<T>(string email, string password, ISignUpOptions options = null) where T : ISession;
        /// <summary>
        /// Signs up with phone.
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<ISession> SignUpWithPhone(string phone, string password, ISignUpOptions options = null);
        /// <summary>
        /// Signs up with phone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phone">The phone.</param>
        /// <param name="password">The password.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        Task<T> SignUpWithPhone<T>(string phone, string password, ISignUpOptions options = null) where T : ISession;
        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<IUser> UpdateUser(string jwt, IUserAttributes attributes);
        /// <summary>
        /// Updates the user.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        Task<T> UpdateUser<T>(string jwt, IUserAttributes attributes) where T : IUser;
        /// <summary>
        /// Updates the user by identifier.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        Task<IUser> UpdateUserById(string jwt, string userId, IUserAttributes userData);
        /// <summary>
        /// Updates the user by identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt">The JWT.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        /// <returns></returns>
        Task<T> UpdateUserById<T>(string jwt, string userId, IUserAttributes userData) where T : IUser;
        /// <summary>
        /// Verifies the mobile otp.
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<ISession> VerifyMobileOTP(string phone, string token);
        /// <summary>
        /// Verifies the mobile otp.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="phone">The phone.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        Task<T> VerifyMobileOTP<T>(string phone, string token) where T : ISession;
    }
}