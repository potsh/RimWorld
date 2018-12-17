using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IngestionOutcomeDoer_GiveHediff : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;

		private bool divideByBodySize;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float effect = (!(severity > 0f)) ? hediffDef.initialSeverity : severity;
			if (divideByBodySize)
			{
				effect /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref effect);
			hediff.Severity = effect;
			pawn.health.AddHediff(hediff);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && chance >= 1f)
			{
				using (IEnumerator<StatDrawEntry> enumerator = hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						StatDrawEntry s = enumerator.Current;
						yield return s;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_00e8:
			/*Error near IL_00e9: Unexpected return in MoveNext()*/;
		}
	}
}
