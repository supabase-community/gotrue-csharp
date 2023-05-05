using System;
using System.Collections.Generic;

namespace Supabase.Gotrue
{
	public class DebugNotification
	{
		private readonly List<Action<string, Exception?>> _debugListeners = new List<Action<string, Exception?>>();

		public void AddDebugListener(Action<string, Exception?> listener)
		{
			_debugListeners.Add(listener);
		}

		public void Log(string message, Exception? e = null)
		{
			foreach (var l in _debugListeners)
			{
				l.Invoke(message, e);
			}
		}
	}
}
