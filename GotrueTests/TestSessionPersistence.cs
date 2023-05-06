using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace GotrueTests
{
	public class TestSessionPersistence : IGotrueSessionPersistence
	{

		public Session SavedSession;

		public void DestroySession()
		{
			SavedSession = null;
		}

		public Session LoadSession()
		{
			return SavedSession;
		}

		public void SaveSession(Session session)
		{
			SavedSession = session;
		}
	}
}
