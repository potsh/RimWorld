using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Exhaustion : Alert
	{
		private IEnumerable<Pawn> ExhaustedColonists => from p in PawnsFinder.AllMaps_FreeColonistsSpawned
		where p.needs.rest != null && p.needs.rest.CurCategory == RestCategory.Exhausted
		select p;

		public Alert_Exhaustion()
		{
			defaultLabel = "Exhaustion".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn exhaustedColonist in ExhaustedColonists)
			{
				stringBuilder.AppendLine("    " + exhaustedColonist.LabelShort);
			}
			return "ExhaustionDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ExhaustedColonists);
		}
	}
}
