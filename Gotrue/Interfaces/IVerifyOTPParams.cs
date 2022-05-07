namespace Supabase.Gotrue.Interfaces
{
    public interface IVerifyOTPParams
    {
        string Email { get; set; }
        string Phone { get; set; }
        string Token { get; set; }
        string Type { get; set; }
    }
}