using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Surroundings : TaleData
	{
		public int tile;

		public float temperature;

		public float snowDepth;

		public WeatherDef weather;

		public RoomRoleDef roomRole;

		public float roomImpressiveness;

		public float roomBeauty;

		public float roomCleanliness;

		public bool Outdoors => weather != null;

		public override void ExposeData()
		{
			Scribe_Values.Look(ref tile, "tile", 0);
			Scribe_Values.Look(ref temperature, "temperature", 0f);
			Scribe_Values.Look(ref snowDepth, "snowDepth", 0f);
			Scribe_Defs.Look(ref weather, "weather");
			Scribe_Defs.Look(ref roomRole, "roomRole");
			Scribe_Values.Look(ref roomImpressiveness, "roomImpressiveness", 0f);
			Scribe_Values.Look(ref roomBeauty, "roomBeauty", 0f);
			Scribe_Values.Look(ref roomCleanliness, "roomCleanliness", 0f);
		}

		public override IEnumerable<Rule> GetRules()
		{
			yield return (Rule)new Rule_String("BIOME", Find.WorldGrid[tile].biome.label);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static TaleData_Surroundings GenerateFrom(IntVec3 c, Map map)
		{
			TaleData_Surroundings taleData_Surroundings = new TaleData_Surroundings();
			taleData_Surroundings.tile = map.Tile;
			Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
			if (roomOrAdjacent != null)
			{
				if (roomOrAdjacent.PsychologicallyOutdoors)
				{
					taleData_Surroundings.weather = map.weatherManager.CurWeatherPerceived;
				}
				taleData_Surroundings.roomRole = roomOrAdjacent.Role;
				taleData_Surroundings.roomImpressiveness = roomOrAdjacent.GetStat(RoomStatDefOf.Impressiveness);
				taleData_Surroundings.roomBeauty = roomOrAdjacent.GetStat(RoomStatDefOf.Beauty);
				taleData_Surroundings.roomCleanliness = roomOrAdjacent.GetStat(RoomStatDefOf.Cleanliness);
			}
			if (!GenTemperature.TryGetTemperatureForCell(c, map, out taleData_Surroundings.temperature))
			{
				taleData_Surroundings.temperature = 21f;
			}
			taleData_Surroundings.snowDepth = map.snowGrid.GetDepth(c);
			return taleData_Surroundings;
		}

		public static TaleData_Surroundings GenerateRandom(Map map)
		{
			return GenerateFrom(CellFinder.RandomCell(map), map);
		}
	}
}
