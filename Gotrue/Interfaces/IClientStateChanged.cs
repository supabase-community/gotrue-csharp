namespace Supabase.Gotrue.Interfaces
{
    public interface IClientStateChanged
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        Client.AuthState State { get; }
    }
}