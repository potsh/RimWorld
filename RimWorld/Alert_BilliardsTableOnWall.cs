using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BilliardsTableOnWall : Alert
	{
		private IEnumerable<Thing> BadTables
		{
			get
			{
				List<Map> maps = Find.Maps;
				Faction ofPlayer = Faction.OfPlayer;
				for (int j = 0; j < maps.Count; j++)
				{
					List<Thing> bList = maps[j].listerThings.ThingsOfDef(ThingDefOf.BilliardsTable);
					for (int i = 0; i < bList.Count; i++)
					{
						if (bList[i].Faction == ofPlayer && !JoyGiver_PlayBilliards.ThingHasStandableSpaceOnAllSides(bList[i]))
						{
							yield return bList[i];
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
		}

		public Alert_BilliardsTableOnWall()
		{
			defaultLabel = "BilliardsNeedsSpace".Translate();
			defaultExplanation = "BilliardsNeedsSpaceDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadTables);
		}
	}
}
