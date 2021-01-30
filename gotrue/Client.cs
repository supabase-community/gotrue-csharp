using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Supabase.Gotrue.Attributes;
using Supabase.Gotrue.Responses;
using static Supabase.Gotrue.Client;

namespace Supabase.Gotrue
{
    public class Client
    {
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

        public enum AuthState
        {
            [MapTo("SIGNED_IN")]
            SignedIn,
            [MapTo("SIGNED_OUT")]
            SignedOut,
            [MapTo("USER_UPDATED")]
            UserUpdated,
            [MapTo("PASSWORD_RECOVERY")]
            PasswordRecovery,
        };

        private static Client instance;
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

        public EventHandler<ClientStateChanged> StateChanged;

        public User CurrentUser { get; private set; }
        public Session CurrentSession { get; private set; }

        protected bool AutoRefreshToken { get; private set; }
        protected bool DetectSessionInUrl { get; private set; }
        protected bool ShouldPersistSession { get; private set; }
        protected Func<Session, bool> SessionPersistor { get; private set; }
        protected Func<bool> SessionDestroyer { get; private set; }

        private Timer refreshTimer = null;

        private Api api;

        private Client() { }

        public static Client Initialize(ClientOptions options = null)
        {
            var instance = new Client();

            if (options == null)
                options = new ClientOptions();

            instance.AutoRefreshToken = options.AutoRefreshToken;
            instance.DetectSessionInUrl = options.DetectSessionInUrl;
            instance.ShouldPersistSession = options.PersistSession;
            instance.SessionPersistor = options.SessionPersistor;
            instance.SessionDestroyer = options.SessionDestroyer;

            instance.api = new Api(options.Url, options.Headers);

            return instance;
        }

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

        public string SignIn(Provider provider)
        {
            DestroySession();

            var url = api.GetUrlForProvider(provider);
            return url;
        }

        public async Task SignOut()
        {
            if (CurrentSession != null)
            {
                await api.SignOut(CurrentSession.AccessToken);
                DestroySession();
                StateChanged?.Invoke(this, new ClientStateChanged(AuthState.SignedOut));
            }
        }

        public async Task<User> Update(UserAttributes attributes)
        {
            if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.AccessToken))
                throw new Exception("Not Logged in.");

            var result = await api.UpdateUser(CurrentSession.AccessToken, attributes);

            CurrentUser = result;

            StateChanged?.Invoke(this, new ClientStateChanged(AuthState.UserUpdated));

            return result;
        }

        internal void PersistSession(Session session)
        {
            CurrentSession = session;
            CurrentUser = session.User;

            var expiration = session.ExpiresIn;

            if (AutoRefreshToken && expiration != null)
            {
                refreshTimer = new Timer((obj) =>
                {
                    RefreshToken();
                    refreshTimer.Dispose();
                });
            }

            if (ShouldPersistSession)
                SessionPersistor?.Invoke(session);
        }

        internal void DestroySession()
        {
            CurrentSession = null;
            CurrentUser = null;

            if (ShouldPersistSession)
                SessionDestroyer?.Invoke();
        }

        internal async void RefreshToken()
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

    public class ClientStateChanged : EventArgs
    {
        public AuthState State { get; private set; }

        public ClientStateChanged(AuthState state)
        {
            State = state;
        }
    }

    public class ClientOptions
    {
        public string Url { get; set; } = Constants.GOTRUE_URL;
        public Dictionary<string, string> Headers = new Dictionary<string, string>(Constants.DEFAULT_HEADERS);
        public bool DetectSessionInUrl { get; set; } = true;
        public bool AutoRefreshToken { get; set; } = true;
        public bool PersistSession { get; set; } = true;
        public Func<Session, bool> SessionPersistor = (Session session) => true;
        public Func<bool> SessionDestroyer = () => true;
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
