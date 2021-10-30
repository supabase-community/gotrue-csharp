using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Supabase.Gotrue.Attributes;
using static Supabase.Gotrue.Client;

namespace Supabase.Gotrue
{
    /// <summary>
    /// The Gotrue Client - a singleton class
    /// </summary>
    /// <example>
    /// var client = Supabase.Gotrue.Client.Initialize(options);
    /// var user = await client.SignIn("user@email.com", "fancyPassword");
    /// </example>
    public class Client
    {
        /// <summary>
        /// Specifies the functionality expected from the `SignIn` method
        /// </summary>
        public enum SignInType
        {
            Email,
            Phone,
            RefreshToken,
        }

        /// <summary>
        /// Specifies the functionality expected from the `SignUp` method
        /// </summary>
        public enum SignUpType
        {
            Email,
            Phone
        }

        /// <summary>
        /// Providers available to Supabase
        /// </summary>
        public enum Provider
        {
            [MapTo("apple")]
            Apple,
            [MapTo("azure")]
            Azure,
            [MapTo("bitbucket")]
            Bitbucket,
            [MapTo("discord")]
            Discord,
            [MapTo("github")]
            Github,
            [MapTo("gitlab")]
            Gitlab,
            [MapTo("google")]
            Google,
            [MapTo("facebook")]
            Facebook,
            [MapTo("slack")]
            Slack,
            [MapTo("spotify")]
            Spotify
            [MapTo("twitch")]
            Twitch,
            [MapTo("twitter")]
            Twitter,
        };

        /// <summary>
        /// States that the Auth Client will raise events for.
        /// </summary>
        public enum AuthState
        {
            SignedIn,
            SignedOut,
            UserUpdated,
            PasswordRecovery,
        };

        private static Client instance;
        /// <summary>
        /// Returns the current instance of this client.
        /// </summary>
        public static Client Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new Exception("`Initialize` must be called prior to accessing `Instance`");
                }
                return instance;
            }
        }

        /// <summary>
        /// Event Handler that raises an event when a user signs in, signs out, recovers password, or updates their record.
        /// </summary>
        public event EventHandler<ClientStateChanged> StateChanged;

        /// <summary>
        /// The current User
        /// </summary>
        public User CurrentUser { get; private set; }

        /// <summary>
        /// The current Session
        /// </summary>
        public Session CurrentSession { get; private set; }

        /// <summary>
        /// Should Client Refresh Token Automatically? (via <see cref="ClientOptions"/>)
        /// </summary>
        protected bool AutoRefreshToken { get; private set; }

        /// <summary>
        /// Should Client Persist Session? (via <see cref="ClientOptions"/>)
        /// </summary>
        protected bool ShouldPersistSession { get; private set; }

        /// <summary>
        /// User defined function (via <see cref="ClientOptions"/>) to persist the session.
        /// </summary>
        protected Func<Session, Task<bool>> SessionPersistor { get; private set; }

        /// <summary>
        /// User defined function (via <see cref="ClientOptions"/>) to retrieve the session.
        /// </summary>
        protected Func<Task<Session>> SessionRetriever { get; private set; }

        /// <summary>
        /// User defined function (via <see cref="ClientOptions"/>) to destroy the session.
        /// </summary>
        protected Func<Task<bool>> SessionDestroyer { get; private set; }

        /// <summary>
        /// Internal timer reference for Refreshing Tokens (<see cref="AutoRefreshToken"/>)
        /// </summary>
        private Timer refreshTimer = null;

        private Api api;

        /// <summary>
        /// Private constructor for Singleton initialization
        /// </summary>
        private Client() { }

        /// <summary>
        /// Initializes a Client.
        ///
        /// Though <see cref="ClientOptions"/> <paramref name="options"/> are ... optional, one will likely
        /// need to define, at the very least, <see cref="ClientOptions.Url"/>.
        ///
        /// If awaited, will asyncronously grab the session via <see cref="SessionRetriever"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static void Initialize(ClientOptions options = null, Action<Client> callback = null)
        {
            Task.Run(async () =>
            {
                var client = await InitializeAsync(options);
                callback?.Invoke(client);
            });
        }

        /// <summary>
        /// Initializes a Client Asynchronously.
        ///
        /// Though <see cref="ClientOptions"/> <paramref name="options"/> are ... optional, one will likely
        /// need to define, at the very least, <see cref="ClientOptions.Url"/>.
        ///
        /// If awaited, will asyncronously grab the session via <see cref="SessionRetriever"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<Client> InitializeAsync(ClientOptions options = null)
        {
            instance = new Client();

            if (options == null)
                options = new ClientOptions();

            instance.AutoRefreshToken = options.AutoRefreshToken;
            instance.ShouldPersistSession = options.PersistSession;
            instance.SessionPersistor = options.SessionPersistor;
            instance.SessionRetriever = options.SessionRetriever;
            instance.SessionDestroyer = options.SessionDestroyer;

            instance.api = new Api(options.Url, options.Headers);

            // Retrieve the session
            if (instance.ShouldPersistSession)
                await instance.RetrieveSessionAsync();

            return instance;
        }

        /// <summary>
        /// Signs up a user by email address
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Session> SignUp(string email, string password) => SignUp(SignUpType.Email, email, password);

        /// <summary>
        /// Signs up a user
        /// </summary>
        /// <param name="type">Type of signup</param>
        /// <param name="identifier">Phone or Email</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Session> SignUp(SignUpType type, string identifier, string password)
        {
            await DestroySession();

            try
            {
                Session session = null;
                switch (type)
                {
                    case SignUpType.Email:
                        session = await api.SignUpWithEmail(identifier, password);
                        break;
                    case SignUpType.Phone:
                        session = await api.SignUpWithPhone(identifier, password);
                        break;
                }

                if (session?.User?.ConfirmedAt != null)
                {
                    await PersistSession(session);

                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

                    return CurrentSession;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }


        /// <summary>
        /// Sends a Magic email login link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> SignIn(string email)
        {
            await DestroySession();

            try
            {
                await api.SendMagicLinkEmail(email);
                return true;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Sends a Magic email login link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<bool> SendMagicLink(string email) => SignIn(email);


        /// <summary>
        /// Signs in a User.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Session> SignIn(string email, string password) => SignIn(SignInType.Email, email, password);

        /// <summary>
        /// Log in an existing user, or login via a third-party provider.
        /// </summary>
        /// <param name="type">Type of Credentials being passed</param>
        /// <param name="identifierOrToken">An email, phone, or RefreshToken</param>
        /// <param name="password">Password to account (optional if `RefreshToken`)</param>
        /// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
        /// <returns></returns>
        public async Task<Session> SignIn(SignInType type, string identifierOrToken, string password = null, string scopes = null)
        {
            await DestroySession();

            try
            {
                Session session = null;
                switch (type)
                {
                    case SignInType.Email:
                        session = await api.SignInWithEmail(identifierOrToken, password);
                        break;
                    case SignInType.Phone:
                        if (string.IsNullOrEmpty(password))
                        {
                            var response = await api.SendMobileOTP(identifierOrToken);
                            return null;
                        }
                        else
                        {
                            session = await api.SignInWithPhone(identifierOrToken, password);
                        }
                        break;
                    case SignInType.RefreshToken:
                        CurrentSession = new Session();
                        CurrentSession.RefreshToken = identifierOrToken;

                        await RefreshToken();

                        return CurrentSession;
                }

                if (session?.User?.ConfirmedAt != null)
                {
                    await PersistSession(session);
                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
                    return CurrentSession;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Retrieves a Url to redirect to for signing in with a <see cref="Provider"/>.
        ///
        /// This method will need to be combined with <see cref="GetSessionFromUrl(Uri, bool)"/> when the
        /// Application receives the Oauth Callback.
        /// </summary>
        /// <example>
        /// var client = Supabase.Gotrue.Client.Initialize(options);
        /// var url = client.SignIn(Provider.Github);
        ///
        /// // Do Redirect User
        ///
        /// // Example code
        /// Application.HasRecievedOauth += async (uri) => {
        ///     var session = await client.GetSessionFromUri(uri, true);
        /// }
        /// </example>
        /// <param name="provider"></param>
        /// <param name="scopes">A space-separated list of scopes granted to the OAuth application.</param>
        /// <returns></returns>
        public async Task<string> SignIn(Provider provider, string scopes = null)
        {
            await DestroySession();

            var url = api.GetUrlForProvider(provider, scopes);
            return url;
        }

        /// <summary>
        /// Log in a user given a User supplied OTP received via mobile.
        /// </summary>
        /// <param name="phone">The user's phone number.</param>
        /// <param name="token">Token sent to the user's phone.</param>
        /// <returns></returns>
        public async Task<Session> VerifyOTP(string phone, string token)
        {
            try
            {
                await DestroySession();

                var session = await api.VerifyMobileOTP(phone, token);

                if (session?.AccessToken != null)
                {
                    await PersistSession(session);
                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
                    return session;
                }

                return null;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Signs out a user and invalidates the current token.
        /// </summary>
        /// <returns></returns>
        public async Task SignOut()
        {
            if (CurrentSession != null)
            {
                await api.SignOut(CurrentSession.AccessToken);
                await DestroySession();
                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedOut));
            }
        }

        /// <summary>
        /// Updates a User.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public async Task<User> Update(UserAttributes attributes)
        {
            if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
                throw new Exception("Not Logged in.");

            try
            {
                var result = await api.UpdateUser(CurrentSession.AccessToken, attributes);

                CurrentUser = result;

                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.UserUpdated));

                return result;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Sends an invite email link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
        /// <returns></returns>
        public async Task<bool> InviteUserByEmail(string email, string jwt)
        {
            try
            {
                var response = await api.InviteUserByEmail(email, jwt);
                response.ResponseMessage.EnsureSuccessStatusCode();
                return true;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Deletes a User.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="jwt">this token needs role 'supabase_admin' or 'service_role'</param>
        /// <returns></returns>
        public async Task<bool> DeleteUser(string uid, string jwt)
        {
            try
            {
                var result = await api.DeleteUser(uid, jwt);
                result.ResponseMessage.EnsureSuccessStatusCode();
                return true;
            }
            catch (RequestException ex)
            {
                throw ParseRequestException(ex);
            }
        }

        /// <summary>
        /// Refreshes the currently logged in User's Session.
        /// </summary>
        /// <returns></returns>
        public async Task<Session> RefreshSession()
        {
            if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
                throw new Exception("Not Logged in.");

            await RefreshToken();

            var user = await api.GetUser(CurrentSession.AccessToken);
            CurrentUser = user;

            return CurrentSession;
        }

        /// <summary>
        /// Parses a <see cref="Session"/> out of a <see cref="Uri"/>'s Query parameters.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="storeSession"></param>
        /// <returns></returns>
        public async Task<Session> GetSessionFromUrl(Uri uri, bool storeSession = true)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);

            var errorDescription = query.Get("error_description");

            if (!string.IsNullOrEmpty(errorDescription))
                throw new Exception(errorDescription);

            var accessToken = query.Get("access_token");

            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("No access_token detected.");

            var expiresIn = query.Get("expires_in");

            if (string.IsNullOrEmpty(expiresIn))
                throw new Exception("No expires_in detected.");

            var refreshToken = query.Get("refresh_token");

            if (string.IsNullOrEmpty(refreshToken))
                throw new Exception("No refresh_token detected.");

            var tokenType = query.Get("token_type");

            if (string.IsNullOrEmpty(tokenType))
                throw new Exception("No token_type detected.");

            var user = await api.GetUser(accessToken);

            var session = new Session
            {
                AccessToken = accessToken,
                ExpiresIn = int.Parse(expiresIn),
                RefreshToken = refreshToken,
                TokenType = tokenType,
                User = user
            };

            if (storeSession)
            {
                await PersistSession(session);
                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

                if (query.Get("type") == "recovery")
                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.PasswordRecovery));
            }

            return session;
        }

        /// <summary>
        /// Retrieves the Session by calling <see cref="SessionRetriever"/> - sets internal state and timers.
        /// </summary>
        /// <returns></returns>
        public async Task<Session> RetrieveSessionAsync()
        {
            if (SessionRetriever == null) return null;

            var session = await SessionRetriever?.Invoke();

            if (session != null && session.ExpiresAt() < DateTime.Now)
            {
                if (AutoRefreshToken && session.RefreshToken != null)
                {
                    try
                    {
                        await RefreshToken();
                        return CurrentSession;
                    }
                    catch
                    {
                        await DestroySession();
                        return null;
                    }
                }
                else
                {
                    await DestroySession();
                    return null;
                }
            }
            else if (session == null || session.User == null)
            {
                Debug.WriteLine("Stored Session is missing data.");
                await DestroySession();
                return null;
            }
            else
            {
                CurrentSession = session;
                CurrentUser = session.User;

                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

                InitRefreshTimer();

                return CurrentSession;
            }
        }

        /// <summary>
        /// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionPersistor"/>
        /// </summary>
        /// <param name="session"></param>
        internal async Task PersistSession(Session session)
        {
            CurrentSession = session;
            CurrentUser = session.User;

            var expiration = session.ExpiresIn;

            if (AutoRefreshToken && expiration != default)
                InitRefreshTimer();

            if (ShouldPersistSession)
                await SessionPersistor?.Invoke(session);
        }

        /// <summary>
        /// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionDestroyer"/>
        /// </summary>
        internal async Task DestroySession()
        {
            CurrentSession = null;
            CurrentUser = null;

            if (ShouldPersistSession)
                await SessionDestroyer?.Invoke();
        }

        /// <summary>
        /// Refreshes a Token
        /// </summary>
        /// <returns></returns>
        internal async Task RefreshToken()
        {
            if (string.IsNullOrEmpty(CurrentSession.RefreshToken))
                throw new Exception("No current session.");

            var result = await api.RefreshAccessToken(CurrentSession.RefreshToken);

            if (string.IsNullOrEmpty(result.AccessToken))
                throw new Exception("Could not refresh token from provided session.");

            CurrentSession = result;
            CurrentUser = result.User;

            StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

            if (AutoRefreshToken && CurrentSession.ExpiresIn != default)
                InitRefreshTimer();
        }

        internal void InitRefreshTimer()
        {
            if (CurrentSession == null || CurrentSession.ExpiresIn == default) return;

            if (refreshTimer != null)
                refreshTimer.Dispose();

            try
            {
                int timeoutSeconds = Convert.ToInt32((CurrentSession.CreatedAt.AddSeconds(CurrentSession.ExpiresIn - 60) - DateTime.Now).TotalSeconds);
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);

                refreshTimer = new Timer(async (obj) =>
                {
                    refreshTimer.Dispose();
                    await RefreshToken();
                }, null, timeout, Timeout.InfiniteTimeSpan);
            }
            catch
            {
                Debug.WriteLine("Unable to parse session timestamp, refresh timer will not work. If persisting, open issue on Github");
            }
        }

        private Exception ParseRequestException(RequestException ex)
        {
            switch (ex.Response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    Debug.WriteLine(ex.Message);
                    return new UnauthorizedException(ex);
                case System.Net.HttpStatusCode.BadRequest:
                    Debug.WriteLine(ex.Message);
                    return new BadRequestException(ex);
                case System.Net.HttpStatusCode.Forbidden:
                    Debug.WriteLine("Forbidden, are sign-ups disabled?");
                    return new ForbiddenException(ex);
            }
            return ex;
        }
    }

    /// <summary>
    /// Class representing a state change on the <see cref="Client"/>.
    /// </summary>
    public class ClientStateChanged : EventArgs
    {
        public AuthState State { get; private set; }

        public ClientStateChanged(AuthState state)
        {
            State = state;
        }
    }

    /// <summary>
    /// Class represention options available to the <see cref="Client"/>.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Gotrue Endpoint
        /// </summary>
        public string Url { get; set; } = Constants.GOTRUE_URL;

        /// <summary>
        /// Headers to be sent with subsequent requests.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>(Constants.DEFAULT_HEADERS);

        /// <summary>
        /// Should the Client automatically handle refreshing the User's Token?
        /// </summary>
        public bool AutoRefreshToken { get; set; } = true;

        /// <summary>
        /// Should the Client call <see cref="SessionPersistor"/>, <see cref="SessionRetriever"/>, and <see cref="SessionDestroyer"/>?
        /// </summary>
        public bool PersistSession { get; set; } = true;

        /// <summary>
        /// Function called to persist the session (probably on a filesystem or cookie)
        /// </summary>
        public Func<Session, Task<bool>> SessionPersistor = (Session session) => Task.FromResult<bool>(true);

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        public Func<Task<Session>> SessionRetriever = () => Task.FromResult<Session>(null);

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        public Func<Task<bool>> SessionDestroyer = () => Task.FromResult<bool>(true);
    }

    public class UnauthorizedException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public string Content { get; private set; }
        public UnauthorizedException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class BadRequestException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public string Content { get; private set; }
        public BadRequestException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class ForbiddenException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public ForbiddenException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class InvalidEmailOrPasswordException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public InvalidEmailOrPasswordException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }

    public class ExistingUserException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public string Content { get; private set; }
        public ExistingUserException(RequestException exception)
        {
            Response = exception.Response;
            Content = exception.Error.Message;
        }
    }
}
