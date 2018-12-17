using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanNightRestUtility
	{
		public const float WakeUpHour = 6f;

		public const float RestStartHour = 22f;

		public static bool RestingNowAt(int tile)
		{
			return WouldBeRestingAt(tile, GenTicks.TicksAbs);
		}

		public static bool WouldBeRestingAt(int tile, long ticksAbs)
		{
			Vector2 vector = Find.WorldGrid.LongLatOf(tile);
			float num = GenDate.HourFloat(ticksAbs, vector.x);
			return num < 6f || num > 22f;
		}

		public static int LeftRestTicksAt(int tile)
		{
			return LeftRestTicksAt(tile, GenTicks.TicksAbs);
		}

		public static int LeftRestTicksAt(int tile, long ticksAbs)
		{
			if (!WouldBeRestingAt(tile, ticksAbs))
			{
				return 0;
			}
			Vector2 vector = Find.WorldGrid.LongLatOf(tile);
			float num = GenDate.HourFloat(ticksAbs, vector.x);
			if (num < 6f)
			{
				return Mathf.CeilToInt((6f - num) * 2500f);
			}
			return Mathf.CeilToInt((24f - num + 6f) * 2500f);
		}

		public static int LeftNonRestTicksAt(int tile)
		{
			return LeftNonRestTicksAt(tile, GenTicks.TicksAbs);
		}

		public static int LeftNonRestTicksAt(int tile, long ticksAbs)
		{
			if (WouldBeRestingAt(tile, ticksAbs))
			{
				return 0;
			}
			Vector2 vector = Find.WorldGrid.LongLatOf(tile);
			float num = GenDate.HourFloat(ticksAbs, vector.x);
			return Mathf.CeilToInt((22f - num) * 2500f);
		}
	}
}
