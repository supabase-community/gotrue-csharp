﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Supabase.Gotrue.Attributes;
using Supabase.Gotrue.Responses;
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
        /// Providers available to Supabase
        /// </summary>
        public enum Provider
        {
            [MapTo("bitbucket")]
            Bitbucket,
            [MapTo("github")]
            Github,
            [MapTo("gitlab")]
            Gitlab,
            [MapTo("google")]
            Google,
            [MapTo("azure")]
            Azure
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
        public EventHandler<ClientStateChanged> StateChanged;

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
        protected Func<Session, bool> SessionPersistor { get; private set; }

        /// <summary>
        /// User defined function (via <see cref="ClientOptions"/>) to retrieve the session.
        /// </summary>
        protected Func<Session> SessionRetriever { get; private set; }

        /// <summary>
        /// User defined function (via <see cref="ClientOptions"/>) to destroy the session.
        /// </summary>
        protected Func<bool> SessionDestroyer { get; private set; }

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
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Client Initialize(ClientOptions options = null)
        {
            var instance = new Client();

            if (options == null)
                options = new ClientOptions();

            instance.AutoRefreshToken = options.AutoRefreshToken;
            instance.ShouldPersistSession = options.PersistSession;
            instance.SessionPersistor = options.SessionPersistor;
            instance.SessionRetriever = options.SessionRetriever;
            instance.SessionDestroyer = options.SessionDestroyer;

            instance.api = new Api(options.Url, options.Headers);

            return instance;
        }

        /// <summary>
        /// Signs up a user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Session> SignUp(string email, string password)
        {
            DestroySession();

            try
            {
                var result = await api.SignUpWithEmail(email, password);

                if (result.User.ConfirmedAt != null)
                {
                    PersistSession(result);

                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

                    return CurrentSession;
                }
                return null;
            }
            catch (RequestException ex)
            {
                Debug.WriteLine("Unable to process signup. Does the user already exist?");
                throw new ExistingUserException(ex);
            }
        }

        /// <summary>
        /// Sends a Magic email login link to the specified email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> SignIn(string email)
        {
            DestroySession();

            try
            {
                var result = await api.SendMagicLinkEmail(email);
                return true;
            }
            catch (RequestException ex)
            {
                throw new InvalidEmailOrPasswordException(ex);
            }
        }

        /// <summary>
        /// Signs in a User.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Session> SignIn(string email, string password)
        {
            DestroySession();

            try
            {
                var result = await api.SignInWithEmail(email, password);

                if (result.User.ConfirmedAt != null)
                {
                    PersistSession(result);
                    StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));
                }

                return result;
            }
            catch (RequestException ex)
            {
                throw new InvalidEmailOrPasswordException(ex);
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
        /// <returns></returns>
        public string SignIn(Provider provider)
        {
            DestroySession();

            var url = api.GetUrlForProvider(provider);
            return url;
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
                DestroySession();
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

            var result = await api.UpdateUser(CurrentSession.AccessToken, attributes);

            CurrentUser = result;

            StateChanged?.Invoke(this, new ClientStateChanged(AuthState.UserUpdated));

            return result;
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
                PersistSession(session);

            return session;
        }

        /// <summary>
        /// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionPersistor"/>
        /// </summary>
        /// <param name="session"></param>
        internal void PersistSession(Session session)
        {
            CurrentSession = session;
            CurrentUser = session.User;

            var expiration = session.ExpiresIn;

            if (AutoRefreshToken && expiration != default)
            {
                refreshTimer = new Timer(async (obj) =>
                {
                    await RefreshToken();
                    refreshTimer.Dispose();
                });
            }

            if (ShouldPersistSession)
                SessionPersistor?.Invoke(session);
        }

        /// <summary>
        /// Persists a Session in memory and calls (if specified) <see cref="ClientOptions.SessionDestroyer"/>
        /// </summary>
        internal void DestroySession()
        {
            CurrentSession = null;
            CurrentUser = null;

            if (ShouldPersistSession)
                SessionDestroyer?.Invoke();
        }

        /// <summary>
        /// Refreshes a Token
        /// </summary>
        /// <returns></returns>
        internal async Task RefreshToken()
        {
            if (string.IsNullOrEmpty(CurrentSession.RefreshToken))
                throw new Exception("No current session.");
            try
            {
                var result = await api.RefreshAccessToken(CurrentSession.RefreshToken);

                if (string.IsNullOrEmpty(result.AccessToken))
                    throw new Exception("Could not refresh token from provided session.");

                CurrentSession = result;
                CurrentUser = result.User;

                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedIn));

                // Todo: Setup timer
            }
            catch (Exception ex) { }
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
        public Func<Session, bool> SessionPersistor = null;

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        public Func<Session> SessionRetriever = null;

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        public Func<bool> SessionDestroyer = null;
    }

    public class InvalidEmailOrPasswordException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public InvalidEmailOrPasswordException(RequestException exception)
        {
            Response = exception.Response;
        }
    }

    public class ExistingUserException : Exception
    {
        public HttpResponseMessage Response { get; private set; }
        public ExistingUserException(RequestException exception)
        {
            Response = exception.Response;
        }
    }
}
