using System;
using System.Net.Http;
using System.Threading;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants.AuthState;

namespace Supabase.Gotrue
{
	/// <summary>
	/// Manages the auto-refresh of the Gotrue Session.
	/// </summary>
	public class TokenRefresh
	{
		private readonly Client _client;

		/// <summary>
		/// Sets up the TokenRefresh class, bound to a specific client
		/// </summary>
		/// <param name="client"></param>
		public TokenRefresh(Client client)
		{
			_client = client;
		}

		/// <summary>
		/// Internal timer reference for token refresh
		/// <see>
		///     <cref>AutoRefreshToken</cref>
		/// </see>
		/// </summary>
		private Timer? _refreshTimer;

		/// <summary>
		/// Turns the auto-refresh timer on or off based on the current auth state
		/// </summary>
		/// <param name="sender">The Client and Session data</param>
		/// <param name="stateChanged"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void ManageAutoRefresh(IGotrueClient<User, Session> sender, Constants.AuthState stateChanged)
		{
			switch (stateChanged)
			{
				case SignedIn:
					_client.Debug("Refresh Timer started");
					InitRefreshTimer();
					// Turn on auto-refresh timer
					break;
				case SignedOut:
					_client.Debug("Refresh Timer stopped");
					_refreshTimer?.Dispose();
					// Turn off auto-refresh timer
					break;
				case UserUpdated:
					_client.Debug("Refresh Timer restarted");
					InitRefreshTimer();
					break;
				case PasswordRecovery:
					// Doesn't affect auto refresh
					break;
				case TokenRefreshed:
					// Doesn't affect auto refresh
					break;
				default: throw new ArgumentOutOfRangeException(nameof(stateChanged), stateChanged, null);
			}
		}

		/// <summary>
		/// Sets up the auto-refresh timer
		/// </summary>
		private void InitRefreshTimer()
		{
			if (_client.CurrentSession == null || _client.CurrentSession.ExpiresIn == default)
			{
				_client.Debug($"No session, refresh timer not started");
				return;
			}

			_refreshTimer?.Dispose();

			if (_client.CurrentSession.Expired())
			{
				_client.Debug($"Token expired, signing out");
				_client.NotifyAuthStateChange(SignedOut);
				return;
			}

			try
			{
				// Interval should be t - (1/5(n)) (i.e. if session time (t) 3600s, attempt refresh at 2880s or 720s (1/5) seconds before expiration)
				var interval = (int)Math.Floor(_client.CurrentSession.ExpiresIn * 4.0f / 5.0f);

				var timeoutSeconds = Convert.ToInt32((_client.CurrentSession.CreatedAt.AddSeconds(interval) - DateTime.Now).TotalSeconds);

				if (timeoutSeconds > _client.Options.MaximumRefreshWaitTime)
					timeoutSeconds = _client.Options.MaximumRefreshWaitTime;

				var timeout = TimeSpan.FromSeconds(timeoutSeconds);

				_refreshTimer = new Timer(HandleRefreshTimerTick, null, timeout, Timeout.InfiniteTimeSpan);

				_client.Debug($"Refresh timer scheduled {timeout.TotalMinutes} minutes");
			}
			catch (Exception e)
			{
				_client.Debug($"Failed to initialize refresh timer", e);
			}
		}

		/// <summary>
		/// This is the background thread that is set up and runs to refresh the token.
		///
		/// This thread is set up to run every five seconds. If the user is offline,
		/// it won't try to refresh the token.
		/// </summary>
		/// <param name="_"></param>
		private async void HandleRefreshTimerTick(object _)
		{
			_refreshTimer?.Dispose();

			try
			{
				if (_client.Online)
					await _client.RefreshToken();
			}
			catch (Exception ex)
			{
				// Something unusually bad happened!
				_client.Debug(ex.Message, ex);
			}
			// Due is set to 5000ms
			_refreshTimer = new Timer(HandleRefreshTimerTick, null, 5000, -1);
		}
	}
}
