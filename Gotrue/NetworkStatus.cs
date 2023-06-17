using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Supabase.Gotrue
{
	/// <summary>
	/// A Network status system to pair with the <see cref="Client.Online"/>Client.
	///
	/// <see cref="https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/network-info"/>
	/// </summary>
	public class NetworkStatus
	{
		private readonly Client _client;
		/// <summary>
		/// Set up a listener for the network status.
		/// </summary>
		/// <param name="client"></param>
		public NetworkStatus(Client client)
		{
			_client = client;
		}

		/// <summary>
		/// Pings the URL in the <see cref="Client.Options"/> to check if the network is online.
		/// </summary>
		public async Task PingCheck()
		{
			var ping = new Ping();
			var hostName = new Uri(_client.Options.Url).Host;
			try
			{
				var reply = await ping.SendPingAsync(hostName);
				if (reply is { Status: IPStatus.Success })
				{
					_client.Debug($"Network Online: {true}");
					_client.Online = true;
				}
				else
				{
					_client.Debug($"Network Problem: {reply.Status}");
					_client.Online = false;
				}
			}
			catch (PingException e)
			{
				_client.Debug($"Network Problem: {e.Message}");
				_client.Online = false;
			}
		}

		/// <summary>
		/// Starts the network status system. This will listen to the OS for network changes,
		/// and also does a ping check to confirm the current network status.
		/// </summary>
		public async Task StartAsync()
		{
			NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
			await PingCheck();
		}

		private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			_client.Debug($"Network Online: {e.IsAvailable}");
			_client.Online = e.IsAvailable;
		}
	}
}
