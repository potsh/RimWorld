using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IngestibleProperties
	{
		[Unsaved]
		public ThingDef parent;

		public int maxNumToIngestAtOnce = 20;

		public List<IngestionOutcomeDoer> outcomeDoers;

		public int baseIngestTicks = 500;

		public float chairSearchRadius = 32f;

		public bool useEatingSpeedStat = true;

		public ThoughtDef tasteThought;

		public ThoughtDef specialThoughtDirect;

		public ThoughtDef specialThoughtAsIngredient;

		public EffecterDef ingestEffect;

		public EffecterDef ingestEffectEat;

		public SoundDef ingestSound;

		[MustTranslate]
		public string ingestCommandString;

		[MustTranslate]
		public string ingestReportString;

		[MustTranslate]
		public string ingestReportStringEat;

		public HoldOffsetSet ingestHoldOffsetStanding;

		public bool ingestHoldUsesTable = true;

		public FoodTypeFlags foodType;

		public float joy;

		public JoyKindDef joyKind;

		public ThingDef sourceDef;

		public FoodPreferability preferability;

		public bool nurseable;

		public float optimalityOffsetHumanlikes;

		public float optimalityOffsetFeedingAnimals;

		public DrugCategory drugCategory;

		[Unsaved]
		private float cachedNutrition = -1f;

		public JoyKindDef JoyKind => (joyKind == null) ? JoyKindDefOf.Gluttonous : joyKind;

		public bool HumanEdible => (FoodTypeFlags.OmnivoreHuman & foodType) != FoodTypeFlags.None;

		public bool IsMeal => (int)preferability >= 6 && (int)preferability <= 9;

		public float CachedNutrition
		{
			get
			{
				if (cachedNutrition == -1f)
				{
					cachedNutrition = parent.GetStatValueAbstract(StatDefOf.Nutrition);
				}
				return cachedNutrition;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (preferability == FoodPreferability.Undefined)
			{
				yield return "undefined preferability";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (foodType == FoodTypeFlags.None)
			{
				yield return "no foodType";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (parent.GetStatValueAbstract(StatDefOf.Nutrition) == 0f && preferability != FoodPreferability.NeverForNutrition)
			{
				yield return "Nutrition == 0 but preferability is " + preferability + " instead of " + FoodPreferability.NeverForNutrition;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!parent.IsCorpse && (int)preferability > 3 && !parent.socialPropernessMatters && parent.EverHaulable)
			{
				yield return "ingestible preferability > DesperateOnlyForHumanlikes but socialPropernessMatters=false. This will cause bugs wherein wardens will look in prison cells for food to give to prisoners and so will repeatedly pick up and drop food inside the cell.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (joy > 0f && joyKind == null)
			{
				yield return "joy > 0 with no joy kind";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (joy == 0f && joyKind != null)
			{
				yield return "joy is 0 but joyKind is " + joyKind;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		internal IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (joy > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Joy".Translate(), joy.ToStringPercent("F2") + " (" + JoyKind.label + ")", 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (outcomeDoers != null)
			{
				for (int i = 0; i < outcomeDoers.Count; i++)
				{
					using (IEnumerator<StatDrawEntry> enumerator = outcomeDoers[i].SpecialDisplayStats(parent).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							StatDrawEntry s = enumerator.Current;
							yield return s;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_019a:
			/*Error near IL_019b: Unexpected return in MoveNext()*/;
		}
	}
}
