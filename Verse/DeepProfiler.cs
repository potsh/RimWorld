using System.Collections.Generic;
using System.Threading;

namespace Verse
{
	public static class DeepProfiler
	{
		private static Dictionary<int, ThreadLocalDeepProfiler> deepProfilers = new Dictionary<int, ThreadLocalDeepProfiler>();

		private static readonly object DeepProfilersLock = new object();

		public static ThreadLocalDeepProfiler Get()
		{
			lock (DeepProfilersLock)
			{
				int managedThreadId = Thread.CurrentThread.ManagedThreadId;
				if (!deepProfilers.TryGetValue(managedThreadId, out ThreadLocalDeepProfiler value))
				{
					value = new ThreadLocalDeepProfiler();
					deepProfilers.Add(managedThreadId, value);
					return value;
				}
				return value;
			}
		}

		public static void Start(string label = null)
		{
			if (Prefs.LogVerbose)
			{
				Get().Start(label);
			}
		}

		public static void End()
		{
			if (Prefs.LogVerbose)
			{
				Get().End();
			}
		}
	}
}
