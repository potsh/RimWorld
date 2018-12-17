using System.Collections.Generic;

namespace Verse
{
	public static class MessagesRepeatAvoider
	{
		private static Dictionary<string, float> lastShowTimes = new Dictionary<string, float>();

		public static void Reset()
		{
			lastShowTimes.Clear();
		}

		public static bool MessageShowAllowed(string tag, float minSecondsSinceLastShow)
		{
			if (!lastShowTimes.TryGetValue(tag, out float value))
			{
				value = -99999f;
			}
			bool flag = RealTime.LastRealTime > value + minSecondsSinceLastShow;
			if (flag)
			{
				lastShowTimes[tag] = RealTime.LastRealTime;
			}
			return flag;
		}
	}
}
