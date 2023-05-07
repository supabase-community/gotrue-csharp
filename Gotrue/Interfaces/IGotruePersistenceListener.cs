namespace Supabase.Gotrue.Interfaces
{
    /// <summary>
    /// Interface for a session persistence auth state handler.
    /// </summary>
    public interface IGotruePersistenceListener<TSession> where TSession : Session
    {
        IGotrueSessionPersistence<TSession> Persistence { get; }

        public void EventHandler(IGotrueClient<User, TSession> sender, Constants.AuthState stateChanged);
    }
}