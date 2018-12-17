using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDefenses : Alert
	{
		public Alert_NeedDefenses()
		{
			defaultLabel = "NeedDefenses".Translate();
			defaultExplanation = "NeedDefensesDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			if (GenDate.DaysPassed < 2 || GenDate.DaysPassed > 5)
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (NeedDefenses(maps[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool NeedDefenses(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			if (!map.mapPawns.AnyColonistSpawned && !map.listerBuildings.allBuildingsColonist.Any())
			{
				return false;
			}
			if (map.listerBuildings.allBuildingsColonist.Any((Building b) => (b.def.building != null && (b.def.building.IsTurret || b.def.building.isTrap)) || b.def == ThingDefOf.Sandbags))
			{
				return false;
			}
			return true;
		}
	}
}
