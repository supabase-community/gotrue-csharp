using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueClientOptions
    {
        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        Dictionary<string, string> Headers { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [allow unconfirmed user sessions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow unconfirmed user sessions]; otherwise, <c>false</c>.
        /// </value>
        bool AllowUnconfirmedUserSessions { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [automatic refresh token].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic refresh token]; otherwise, <c>false</c>.
        /// </value>
        bool AutoRefreshToken { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [persist session].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [persist session]; otherwise, <c>false</c>.
        /// </value>
        bool PersistSession { get; set; }
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        string Url { get; set; }

        /// <summary>
        /// Function called to persist the session (probably on a filesystem or cookie)
        /// </summary>
        /// <value>
        /// The session persistor.
        /// </value>
        Func<ISession, Task<bool>> SessionPersistor { get; set; }

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        /// <value>
        /// The session retriever.
        /// </value>
        Func<Task<ISession>> SessionRetriever { get; set; }

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        /// <value>
        /// The session destroyer.
        /// </value>
        Func<Task<bool>> SessionDestroyer { get; set; }
    }
}