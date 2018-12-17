using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Hypothermia : Alert_Critical
	{
		private IEnumerable<Pawn> HypothermiaDangerColonists
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.SafeTemperatureRange().Includes(item.AmbientTemperature))
					{
						Hediff hypo = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
						if (hypo != null && hypo.CurStageIndex >= 3)
						{
							yield return item;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				yield break;
				IL_0113:
				/*Error near IL_0114: Unexpected return in MoveNext()*/;
			}
		}

		public Alert_Hypothermia()
		{
			defaultLabel = "AlertHypothermia".Translate();
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn hypothermiaDangerColonist in HypothermiaDangerColonists)
			{
				stringBuilder.AppendLine("    " + hypothermiaDangerColonist.LabelShort);
			}
			return "AlertHypothermiaDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(HypothermiaDangerColonists);
		}
	}
}
