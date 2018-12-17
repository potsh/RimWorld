using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class DateNotifier : IExposable
	{
		private Season lastSeason;

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastSeason, "lastSeason", Season.Undefined);
		}

		public void DateNotifierTick()
		{
			Map map = FindPlayerHomeWithMinTimezone();
			float num;
			if (map != null)
			{
				Vector2 vector = Find.WorldGrid.LongLatOf(map.Tile);
				num = vector.y;
			}
			else
			{
				num = 0f;
			}
			float latitude = num;
			float num2;
			if (map != null)
			{
				Vector2 vector2 = Find.WorldGrid.LongLatOf(map.Tile);
				num2 = vector2.x;
			}
			else
			{
				num2 = 0f;
			}
			float longitude = num2;
			Season season = GenDate.Season(Find.TickManager.TicksAbs, latitude, longitude);
			if (season != lastSeason && (lastSeason == Season.Undefined || season != lastSeason.GetPreviousSeason()))
			{
				if (lastSeason != 0 && AnyPlayerHomeSeasonsAreMeaningful())
				{
					if (GenDate.YearsPassed == 0 && season == Season.Summer && AnyPlayerHomeAvgTempIsLowInWinter())
					{
						Find.LetterStack.ReceiveLetter("LetterLabelFirstSummerWarning".Translate(), "FirstSummerWarning".Translate(), LetterDefOf.NeutralEvent);
					}
					else if (GenDate.DaysPassed > 5)
					{
						Messages.Message("MessageSeasonBegun".Translate(season.Label()).CapitalizeFirst(), MessageTypeDefOf.NeutralEvent);
					}
				}
				lastSeason = season;
			}
		}

		private Map FindPlayerHomeWithMinTimezone()
		{
			List<Map> maps = Find.Maps;
			Map map = null;
			int num = -1;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					Vector2 vector = Find.WorldGrid.LongLatOf(maps[i].Tile);
					int num2 = GenDate.TimeZoneAt(vector.x);
					if (map == null || num2 < num)
					{
						map = maps[i];
						num = num2;
					}
				}
			}
			return map;
		}

		private bool AnyPlayerHomeSeasonsAreMeaningful()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && maps[i].mapTemperature.LocalSeasonsAreMeaningful())
				{
					return true;
				}
			}
			return false;
		}

		private bool AnyPlayerHomeAvgTempIsLowInWinter()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome)
				{
					int tile = maps[i].Tile;
					Vector2 vector = Find.WorldGrid.LongLatOf(maps[i].Tile);
					if (GenTemperature.AverageTemperatureAtTileForTwelfth(tile, Season.Winter.GetMiddleTwelfth(vector.y)) < 8f)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
