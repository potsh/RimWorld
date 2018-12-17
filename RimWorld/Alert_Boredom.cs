using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Boredom : Alert
	{
		private const float JoyNeedThreshold = 0.24000001f;

		public Alert_Boredom()
		{
			defaultLabel = "Boredom".Translate();
			defaultPriority = AlertPriority.Medium;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BoredPawns());
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Pawn pawn = null;
			foreach (Pawn item in BoredPawns())
			{
				stringBuilder.AppendLine("   " + item.Label);
				if (pawn == null)
				{
					pawn = item;
				}
			}
			string value = JoyUtility.JoyKindsOnMapString(pawn.Map);
			return "BoredomDesc".Translate(stringBuilder.ToString().TrimEndNewlines(), pawn.LabelShort, value, pawn.Named("PAWN"));
		}

		private IEnumerable<Pawn> BoredPawns()
		{
			foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if ((item.needs.joy.CurLevelPercentage < 0.24000001f || item.GetTimeAssignment() == TimeAssignmentDefOf.Joy) && item.needs.joy.tolerances.BoredOfAllAvailableJoyKinds(item))
				{
					yield return item;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_010c:
			/*Error near IL_010d: Unexpected return in MoveNext()*/;
		}
	}
}
