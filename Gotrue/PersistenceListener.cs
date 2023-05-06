using System;
using Supabase.Gotrue.Interfaces;

namespace Supabase.Gotrue
{
	public class PersistenceListener
	{
		public PersistenceListener(IGotrueSessionPersistence persistence)
		{
			Persistence = persistence;
		}

		public IGotrueSessionPersistence Persistence { get; }
		
		public void EventHandler(IGotrueClient<User, Session> sender, Constants.AuthState stateChanged)
		{
			switch (stateChanged)
			{
				case Constants.AuthState.SignedIn:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					Persistence.SaveSession(sender.CurrentSession);
					break;
				case Constants.AuthState.SignedOut:
					Persistence.DestroySession();
					break;
				case Constants.AuthState.UserUpdated:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					Persistence.SaveSession(sender.CurrentSession);
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
						Persistence.SaveSession(sender.CurrentSession);
					}
					break;
				default: throw new ArgumentOutOfRangeException(nameof(stateChanged), stateChanged, null);
			}
		}
	}
}
