using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase.Gotrue.Interfaces;
namespace Supabase.Gotrue
{
	/// <summary>
	/// Admin client for interacting with the Gotrue API. Intended for use on
	/// servers or other secure environments.
	///
	/// This client does NOT manage user sessions or track any other state.
	/// </summary>
	public class AdminClient : IGotrueAdminClient<User>
	{
		/// <summary>
		/// The initialized client options.
		/// </summary>
		public ClientOptions Options { get; }

		/// <summary>
		/// Initialize the client with a service key. 
		/// </summary>
		/// <param name="serviceKey">A valid JWT. Must be a full-access API key (e.g. 'service_role' or 'supabase_admin'). </param>
		/// <param name="options"></param>
		public AdminClient(string serviceKey, ClientOptions? options = null)
		{
			_serviceKey = serviceKey;

			options ??= new ClientOptions();
			Options = options;
			_api = new Api(options.Url, options.Headers);
		}

		/// <summary>
		/// Headers sent to the API on every request.
		/// </summary>
		public Func<Dictionary<string, string>>? GetHeaders
		{
			get => _api.GetHeaders;
			set => _api.GetHeaders = value;
		}

		/// <summary>
		/// The underlying API requests object that sends the requests
		/// </summary>
		private readonly IGotrueApi<User, Session> _api;

		/// <summary>
		/// The service key used to authenticate with the API.
		/// </summary>
		private readonly string _serviceKey;

		/// <inheritdoc />
		public Task<User?> GetUserById(string userId) => _api.GetUserById(_serviceKey, userId);

		/// <inheritdoc />
		public Task<User?> GetUser(string jwt) => _api.GetUser(jwt);

		/// <inheritdoc />
		public async Task<bool> InviteUserByEmail(string email)
		{
			var response = await _api.InviteUserByEmail(email, _serviceKey);
			response.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <inheritdoc />
		public async Task<bool> DeleteUser(string uid)
		{
			var result = await _api.DeleteUser(uid, _serviceKey);
			result.ResponseMessage?.EnsureSuccessStatusCode();
			return true;
		}

		/// <inheritdoc />
		public Task<User?> CreateUser(string email, string password, AdminUserAttributes? attributes = null)
		{
			attributes ??= new AdminUserAttributes();
			attributes.Email = email;
			attributes.Password = password;

			return CreateUser(attributes);
		}

		/// <inheritdoc />
		public Task<User?> CreateUser(AdminUserAttributes attributes) => _api.CreateUser(_serviceKey, attributes);

		/// <inheritdoc />
		public Task<UserList<User>?> ListUsers(string? filter = null, string? sortBy = null, Constants.SortOrder sortOrder = Constants.SortOrder.Descending, int? page = null, int? perPage = null)
		{
			return _api.ListUsers(_serviceKey, filter, sortBy, sortOrder, page, perPage);
		}

		/// <inheritdoc />
		public Task<User?> UpdateUserById(string userId, AdminUserAttributes userData)
		{
			return _api.UpdateUserById(_serviceKey, userId, userData);
		}

		/// <inheritdoc />
		public async Task<User?> Update(UserAttributes attributes)
		{
			var result = await _api.UpdateUser(_serviceKey, attributes);
			return result;
		}
	}
}
