using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_PasteDispenserNeedsHopper : Alert
	{
		private IEnumerable<Thing> BadDispensers
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Building item in maps[i].listerBuildings.allBuildingsColonist)
					{
						if (item.def.IsFoodDispenser)
						{
							bool foundHopper = false;
							ThingDef hopperDef = ThingDefOf.Hopper;
							foreach (IntVec3 item2 in GenAdj.CellsAdjacentCardinal(item))
							{
								if (item2.InBounds(maps[i]))
								{
									Thing building = item2.GetEdifice(item.Map);
									if (building != null && building.def == hopperDef)
									{
										foundHopper = true;
										break;
									}
								}
							}
							if (!foundHopper)
							{
								yield return (Thing)item;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_01ee:
				/*Error near IL_01ef: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_PasteDispenserNeedsHopper()
		{
			defaultLabel = "NeedFoodHopper".Translate();
			defaultExplanation = "NeedFoodHopperDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadDispensers);
		}
	}
}
