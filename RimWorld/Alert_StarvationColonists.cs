using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationColonists : Alert
	{
		private IEnumerable<Pawn> StarvingColonists
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (item.needs.food != null && item.needs.food.Starving)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00e2:
				/*Error near IL_00e3: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_StarvationColonists()
		{
			defaultLabel = "Starvation".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn starvingColonist in StarvingColonists)
			{
				stringBuilder.AppendLine("    " + starvingColonist.LabelShort);
			}
			return "StarvationDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(StarvingColonists);
		}
	}
}
