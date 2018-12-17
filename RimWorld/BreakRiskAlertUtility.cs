using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class BreakRiskAlertUtility
	{
		public static IEnumerable<Pawn> PawnsAtRiskExtreme
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakExtremeIsImminent)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00dd:
				/*Error near IL_00de: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> PawnsAtRiskMajor
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakMajorIsImminent)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00dd:
				/*Error near IL_00de: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> PawnsAtRiskMinor
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakMinorIsImminent)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00dd:
				/*Error near IL_00de: Unexpected return in MoveNext()*/;
			}
		}

		public static string AlertLabel
		{
			get
			{
				int num = PawnsAtRiskExtreme.Count();
				string text;
				if (num > 0)
				{
					text = "BreakRiskExtreme".Translate();
				}
				else
				{
					num = PawnsAtRiskMajor.Count();
					if (num > 0)
					{
						text = "BreakRiskMajor".Translate();
					}
					else
					{
						num = PawnsAtRiskMinor.Count();
						text = "BreakRiskMinor".Translate();
					}
				}
				if (num > 1)
				{
					text = text + " x" + num.ToStringCached();
				}
				return text;
			}
		}

		public static string AlertExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (PawnsAtRiskExtreme.Any())
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					foreach (Pawn item in PawnsAtRiskExtreme)
					{
						stringBuilder2.AppendLine("    " + item.LabelShort);
					}
					stringBuilder.Append("BreakRiskExtremeDesc".Translate(stringBuilder2));
				}
				if (PawnsAtRiskMajor.Any())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder3 = new StringBuilder();
					foreach (Pawn item2 in PawnsAtRiskMajor)
					{
						stringBuilder3.AppendLine("    " + item2.LabelShort);
					}
					stringBuilder.Append("BreakRiskMajorDesc".Translate(stringBuilder3));
				}
				if (PawnsAtRiskMinor.Any())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder4 = new StringBuilder();
					foreach (Pawn item3 in PawnsAtRiskMinor)
					{
						stringBuilder4.AppendLine("    " + item3.LabelShort);
					}
					stringBuilder.Append("BreakRiskMinorDesc".Translate(stringBuilder4));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("BreakRiskDescEnding".Translate());
				return stringBuilder.ToString();
			}
		}
	}
}
