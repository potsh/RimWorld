using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StorytellerDef : Def
	{
		public int listOrder = 9999;

		public bool listVisible = true;

		public bool tutorialMode;

		public bool disableAdaptiveTraining;

		public bool disableAlerts;

		public bool disablePermadeath;

		public DifficultyDef forcedDifficulty;

		[NoTranslate]
		private string portraitLarge;

		[NoTranslate]
		private string portraitTiny;

		public List<StorytellerCompProperties> comps = new List<StorytellerCompProperties>();

		public SimpleCurve populationIntentFactorFromPopCurve;

		public SimpleCurve populationIntentFactorFromPopAdaptDaysCurve;

		public SimpleCurve pointsFactorFromDaysPassed;

		public float adaptDaysMin;

		public float adaptDaysMax = 100f;

		public float adaptDaysGameStartGraceDays;

		public SimpleCurve pointsFactorFromAdaptDays;

		public SimpleCurve adaptDaysLossFromColonistLostByPostPopulation;

		public SimpleCurve adaptDaysLossFromColonistViolentlyDownedByPopulation;

		public SimpleCurve adaptDaysGrowthRateCurve;

		[Unsaved]
		public Texture2D portraitLargeTex;

		[Unsaved]
		public Texture2D portraitTinyTex;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!portraitTiny.NullOrEmpty())
				{
					portraitTinyTex = ContentFinder<Texture2D>.Get(portraitTiny);
					portraitLargeTex = ContentFinder<Texture2D>.Get(portraitLarge);
				}
			});
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].ResolveReferences(this);
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (pointsFactorFromAdaptDays == null)
			{
				yield return "pointsFactorFromAdaptDays is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (adaptDaysLossFromColonistLostByPostPopulation == null)
			{
				yield return "adaptDaysLossFromColonistLostByPostPopulation is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (adaptDaysLossFromColonistViolentlyDownedByPopulation == null)
			{
				yield return "adaptDaysLossFromColonistViolentlyDownedByPopulation is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (adaptDaysGrowthRateCurve == null)
			{
				yield return "adaptDaysGrowthRateCurve is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (pointsFactorFromDaysPassed == null)
			{
				yield return "pointsFactorFromDaysPassed is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e2 = enumerator.Current;
					yield return e2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<string> enumerator2 = comps[i].ConfigErrors(this).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string e = enumerator2.Current;
						yield return e;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0294:
			/*Error near IL_0295: Unexpected return in MoveNext()*/;
		}
	}
}
