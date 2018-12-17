using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistNeedsTend : Alert
	{
		private IEnumerable<Pawn> NeedingColonists
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.health.HasHediffsNeedingTendByPlayer(forAlert: true))
					{
						Building_Bed curBed = item.CurrentBed();
						if ((curBed == null || !curBed.Medical) && !Alert_ColonistNeedsRescuing.NeedsRescue(item))
						{
							yield return item;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				yield break;
				IL_0114:
				/*Error near IL_0115: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_ColonistNeedsTend()
		{
			defaultLabel = "ColonistNeedsTreatment".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn needingColonist in NeedingColonists)
			{
				stringBuilder.AppendLine("    " + needingColonist.LabelShort);
			}
			return "ColonistNeedsTreatmentDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(NeedingColonists);
		}
	}
}
