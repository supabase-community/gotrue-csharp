using System.Net.Http;

namespace Supabase.Gotrue.Interfaces
{
    public interface IBaseResponse
    {
        string Content { get; set; }
        HttpResponseMessage ResponseMessage { get; set; }
    }
}