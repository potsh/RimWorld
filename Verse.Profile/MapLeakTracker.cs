using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Profile
{
	[HasDebugOutput]
	internal static class MapLeakTracker
	{
		private static List<WeakReference<Map>> references = new List<WeakReference<Map>>();

		private static List<WeakReference<Map>> referencesFlagged = new List<WeakReference<Map>>();

		private static float lastUpdateSecond = 0f;

		private static int lastUpdateTick = 0;

		private static bool gcSinceLastUpdate = false;

		private static long gcUsedLastFrame = 0L;

		private const float TimeBetweenUpdateRealtimeSeconds = 60f;

		private const float TimeBetweenUpdateGameDays = 1f;

		private static bool Enabled => UnityData.isDebugBuild;

		public static void AddReference(Map element)
		{
			if (Enabled)
			{
				references.Add(new WeakReference<Map>(element));
			}
		}

		public static void Update()
		{
			if (Enabled)
			{
				if (Current.Game != null && Find.TickManager.TicksGame < lastUpdateTick)
				{
					lastUpdateTick = Find.TickManager.TicksGame;
				}
				long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
				if (totalMemory < gcUsedLastFrame)
				{
					gcSinceLastUpdate = true;
				}
				gcUsedLastFrame = totalMemory;
				if (!(lastUpdateSecond + 60f > Time.time) && (Current.Game == null || !((float)lastUpdateTick + 60000f > (float)Find.TickManager.TicksGame)) && gcSinceLastUpdate)
				{
					lastUpdateSecond = Time.time;
					if (Current.Game != null)
					{
						lastUpdateTick = Find.TickManager.TicksGame;
					}
					gcSinceLastUpdate = false;
					for (int i = 0; i < referencesFlagged.Count; i++)
					{
						WeakReference<Map> weakReference = referencesFlagged[i];
						Map map = weakReference.Target;
						if (map != null && (Find.Maps == null || !Find.Maps.Contains(map)) && Current.ProgramState == ProgramState.Entry)
						{
							Log.Error($"Memory leak detected: map {map.ToStringSafe()} is still live when it shouldn't be; this map will not be mentioned again. For more info set MemoryTrackerMarkup.enableMemoryTracker to true and use \"Object Hold Paths\"->Map debug option.");
							references.RemoveAll((WeakReference<Map> liveref) => liveref.Target == map);
						}
					}
					referencesFlagged.Clear();
					for (int j = 0; j < references.Count; j++)
					{
						WeakReference<Map> weakReference2 = references[j];
						Map target = weakReference2.Target;
						if (target != null && (Find.Maps == null || !Find.Maps.Contains(target)))
						{
							referencesFlagged.Add(weakReference2);
						}
					}
					references.RemoveAll((WeakReference<Map> liveref) => !liveref.IsAlive);
					gcUsedLastFrame = GC.GetTotalMemory(forceFullCollection: false);
				}
			}
		}

		[DebugOutput]
		[Category("System")]
		public static void ForceLeakCheck()
		{
			GC.Collect();
			for (int i = 0; i < references.Count; i++)
			{
				WeakReference<Map> weakReference = references[i];
				Map target = weakReference.Target;
				if (target != null && (Find.Maps == null || !Find.Maps.Contains(target)))
				{
					Log.Error($"Memory leak detected: map {target.ToStringSafe()} is still live when it shouldn't be. For more info set MemoryTrackerMarkup.enableMemoryTracker to true and use \"Object Hold Paths\"->Map debug option.");
				}
			}
		}
	}
}
