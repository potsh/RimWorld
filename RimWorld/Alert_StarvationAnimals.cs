using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationAnimals : Alert
	{
		private IEnumerable<Pawn> StarvingAnimals => from p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep
		where p.HostFaction == null && !p.RaceProps.Humanlike
		where p.needs.food != null && (p.needs.food.TicksStarving > 30000 || (p.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true) && p.needs.food.TicksStarving > 5000))
		select p;

		public Alert_StarvationAnimals()
		{
			defaultLabel = "StarvationAnimals".Translate();
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in from a in StarvingAnimals
			orderby a.def.label
			select a)
			{
				stringBuilder.Append("    " + item.LabelShort.CapitalizeFirst());
				if (item.Name.IsValid && !item.Name.Numerical)
				{
					stringBuilder.Append(" (" + item.def.label + ")");
				}
				stringBuilder.AppendLine();
			}
			return "StarvationAnimalsDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(StarvingAnimals);
		}
	}
}
