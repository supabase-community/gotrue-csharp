using System;
using System.Threading.Tasks;
using Supabase.Gotrue.Interfaces;

namespace Supabase.Gotrue
{
	public class PersistenceListener
	{
		private readonly SaveSession? _save;
		private readonly LoadSession? _load;
		private readonly DestroySession? _destroy;

		public delegate bool SaveSession(Session session);

		public delegate void DestroySession();

		public delegate Session LoadSession();

		public PersistenceListener(SaveSession? s, DestroySession? d, LoadSession? l)
		{
			_save = s;
			_load = l;
			_destroy = d;
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

					_save?.Invoke(sender.CurrentSession);
					break;
				case Constants.AuthState.SignedOut:
					_destroy?.Invoke();
					break;
				case Constants.AuthState.UserUpdated:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					_save?.Invoke(sender.CurrentSession);
					break;
				case Constants.AuthState.PasswordRecovery: break;
				case Constants.AuthState.TokenRefreshed:
					if (sender == null)
						throw new ArgumentException("Tried to save a null session (1)");
					if (sender.CurrentSession == null)
						throw new ArgumentException("Tried to save a null session (2)");

					_save?.Invoke(sender.CurrentSession);
					break;
				case Constants.AuthState.ClientLaunch:
					_load?.Invoke();
					break;
				default: throw new ArgumentOutOfRangeException(nameof(stateChanged), stateChanged, null);
			}
		}
	}
}
