using System;
using Supabase.Gotrue.Interfaces;

namespace Supabase.Gotrue
{
	public class PersistenceListener
	{
		private readonly GotrueSessionPersistence _persistence;

		public delegate bool SaveSession(Session session);

		public delegate void DestroySession();

		public delegate Session LoadSession();

		public PersistenceListener(GotrueSessionPersistence persistence)
		{
			_persistence = persistence;
		}
		public void EventHandler(IGotrueClient<User, Session> sender, Constants.AuthState stateChanged)
		{
			switch (stateChanged)
			{
				case Constants.AuthState.SignedIn:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					_persistence.Save(sender.CurrentSession);
					break;
				case Constants.AuthState.SignedOut:
					_persistence.Destroy.Invoke();
					break;
				case Constants.AuthState.UserUpdated:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					_persistence.Save.Invoke(sender.CurrentSession);
					break;
				case Constants.AuthState.PasswordRecovery: break;
				case Constants.AuthState.TokenRefreshed:
					if (sender.CurrentSession == null)
					{
						// If token refresh results in a null session, log out.
						EventHandler(sender, Constants.AuthState.SignedOut);
					}
					else
					{
						_persistence.Save.Invoke(sender.CurrentSession);
					}
					break;
				default: throw new ArgumentOutOfRangeException(nameof(stateChanged), stateChanged, null);
			}
		}
	}
}
