namespace Supabase.Gotrue.Interfaces
{
	/// <summary>
	/// Interface for session persistence. As a reminder, make sure you handle exceptions and
	/// other error conditions in your implementation.
	/// </summary>
	public interface IGotrueSessionPersistence
	{
		public void SaveSession(Session session);

		public void DestroySession();

		public Session LoadSession();
	}
}
