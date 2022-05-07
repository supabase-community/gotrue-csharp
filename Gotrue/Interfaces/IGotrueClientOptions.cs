using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Supabase.Gotrue.Interfaces
{
    public interface IGotrueClientOptions
    {
        Dictionary<string, string> Headers { get; set; }
        bool AllowUnconfirmedUserSessions { get; set; }
        bool AutoRefreshToken { get; set; }
        bool PersistSession { get; set; }
        string Url { get; set; }

        /// <summary>
        /// Function called to persist the session (probably on a filesystem or cookie)
        /// </summary>
        Func<ISession, Task<bool>> SessionPersistor { get; set; }

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        Func<Task<ISession>> SessionRetriever { get; set; }

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        Func<Task<bool>> SessionDestroyer { get; set; }
    }
}