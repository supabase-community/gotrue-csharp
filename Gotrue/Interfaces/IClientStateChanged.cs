namespace Supabase.Gotrue.Interfaces
{
    public interface IClientStateChanged
    {
        Client.AuthState State { get; }
    }
}