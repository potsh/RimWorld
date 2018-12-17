using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_AwaitingMedicalOperation : Alert
	{
		private IEnumerable<Pawn> AwaitingMedicalOperation => from p in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer).Concat(PawnsFinder.AllMaps_PrisonersOfColonySpawned)
		where HealthAIUtility.ShouldHaveSurgeryDoneNow(p) && p.InBed()
		select p;

		public override string GetLabel()
		{
			return "PatientsAwaitingMedicalOperation".Translate(AwaitingMedicalOperation.Count().ToStringCached());
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in AwaitingMedicalOperation)
			{
				stringBuilder.AppendLine("    " + item.LabelShort.CapitalizeFirst());
			}
			return "PatientsAwaitingMedicalOperationDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(AwaitingMedicalOperation);
		}
	}
}
