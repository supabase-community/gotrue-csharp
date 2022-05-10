namespace Supabase.Gotrue.Interfaces
{
    public interface ISignInOptions
    {
        /// <summary>
        /// Gets or sets the redirect to.
        /// </summary>
        /// <value>
        /// The redirect to.
        /// </value>
        string RedirectTo { get; set; }
    }
}