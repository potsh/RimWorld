using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Drug : CompProperties
	{
		public ChemicalDef chemical;

		public float addictiveness;

		public float minToleranceToAddict;

		public float existingAddictionSeverityOffset = 0.1f;

		public float needLevelOffset = 1f;

		public FloatRange overdoseSeverityOffset = FloatRange.Zero;

		public float largeOverdoseChance;

		public bool isCombatEnhancingDrug;

		public float listOrder;

		public bool Addictive => addictiveness > 0f;

		public bool CanCauseOverdose => overdoseSeverityOffset.TrueMax > 0f;

		public CompProperties_Drug()
		{
			compClass = typeof(CompDrug);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Addictive && chemical == null)
			{
				yield return "addictive but chemical is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0102:
			/*Error near IL_0103: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			using (IEnumerator<StatDrawEntry> enumerator = base.SpecialDisplayStats(req).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StatDrawEntry s = enumerator.Current;
					yield return s;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Addictive)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Addictiveness".Translate(), addictiveness.ToStringPercent(), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0117:
			/*Error near IL_0118: Unexpected return in MoveNext()*/;
		}
	}
}
