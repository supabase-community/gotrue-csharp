namespace Supabase.Gotrue
{
	public class GotrueSessionPersistence
	{
		public delegate bool SaveSession(Session session);

		public delegate void DestroySession();

		public delegate Session LoadSession();

		public readonly SaveSession Save;
		public readonly DestroySession Destroy;
		public readonly LoadSession Load;

		public GotrueSessionPersistence(SaveSession save, LoadSession load, DestroySession destroy)
		{
			Save = save;
			Destroy = destroy;
			Load = load;
		}
	}

}
