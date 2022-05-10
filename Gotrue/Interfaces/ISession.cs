using System;

namespace Supabase.Gotrue.Interfaces
{
    public interface ISession
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        string AccessToken { get; set; }
        /// <summary>
        /// Gets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        DateTime CreatedAt { get; }
        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        int ExpiresIn { get; set; }
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        string RefreshToken { get; set; }
        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        string TokenType { get; set; }
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        User User { get; set; }
        /// <summary>
        /// Expires at.
        /// </summary>
        /// <returns></returns>
        DateTime ExpiresAt();
    }
}