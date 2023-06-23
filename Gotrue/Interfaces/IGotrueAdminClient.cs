using System.Threading.Tasks;
using Supabase.Core.Interfaces;
namespace Supabase.Gotrue.Interfaces
{
	/// <summary>
	/// Interface for the Gotrue Admin Client (auth).
	/// </summary>
	/// <typeparam name="TUser"></typeparam>
	public interface IGotrueAdminClient<TUser> : IGettableHeaders
		where TUser : User
	{
		/// <summary>
		/// Creates a user using the admin key (not the anonymous key).
		/// Used in trusted server environments, not client apps.
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		Task<TUser?> CreateUser(AdminUserAttributes attributes);

		/// <summary>
		/// Creates a user using the admin key (not the anonymous key).
		/// Used in trusted server environments, not client apps.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="password"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		Task<TUser?> CreateUser(string email, string password, AdminUserAttributes? attributes = null);

		/// <summary>
		/// Creates a user using the admin key (not the anonymous key).
		/// Used in trusted server environments, not client apps.
		/// </summary>
		Task<bool> DeleteUser(string uid);

		/// <summary>
		/// Gets a user from a user's JWT. This is using the GoTrue server to validate a user's JWT.
		/// </summary>
		/// <param name="jwt"></param>
		/// <returns></returns>
		Task<TUser?> GetUser(string jwt);
		
		/// <summary>
		/// Gets a user by ID from the server using the admin key (not the anonymous key).
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<TUser?> GetUserById(string userId);

		/// <summary>
		/// Sends an email to the user.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		Task<bool> InviteUserByEmail(string email);

		/// <summary>
		/// Lists users
		/// </summary>
		/// <param name="filter">A string for example part of the email</param>
		/// <param name="sortBy">Snake case string of the given key, currently only created_at is supported</param>
		/// <param name="sortOrder">asc or desc, if null desc is used</param>
		/// <param name="page">page to show for pagination</param>
		/// <param name="perPage">items per page for pagination</param>
		/// <returns></returns>
		Task<UserList<TUser>?> ListUsers(string? filter = null, string? sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null,
			int? perPage = null);

		/// <summary>
		/// Updates a User using the service key
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public Task<User?> Update(UserAttributes attributes);
		
		/// <summary>
		/// Update user by Id
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public Task<User?> UpdateUserById(string userId, AdminUserAttributes userData);
	}
}
