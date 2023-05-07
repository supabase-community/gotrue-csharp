namespace Supabase.Gotrue.Interfaces
{
    /// <summary>
    /// Interface for session persistence. As a reminder, make sure you handle exceptions and
    /// other error conditions in your implementation.
    /// </summary>
    public interface IGotrueSessionPersistence<TSession> where TSession : Session
    {
        public void SaveSession(TSession session);

        public void DestroySession();

        public TSession LoadSession();
    }
}
