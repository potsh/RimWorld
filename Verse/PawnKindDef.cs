using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	public class PawnKindDef : Def
	{
		public ThingDef race;

		public FactionDef defaultFactionType;

		[NoTranslate]
		public List<string> backstoryCategories;

		[MustTranslate]
		public string labelPlural;

		public List<PawnKindLifeStage> lifeStages = new List<PawnKindLifeStage>();

		public float backstoryCryptosleepCommonality;

		public int minGenerationAge;

		public int maxGenerationAge = 999999;

		public bool factionLeader;

		public bool destroyGearOnDrop;

		public bool isFighter = true;

		public float combatPower = -1f;

		public bool canArriveManhunter = true;

		public bool canBeSapper;

		public float baseRecruitDifficulty = 0.5f;

		public bool aiAvoidCover;

		public FloatRange fleeHealthThresholdRange = new FloatRange(-0.4f, 0.4f);

		public QualityCategory itemQuality = QualityCategory.Normal;

		public bool forceNormalGearQuality;

		public FloatRange gearHealthRange = FloatRange.One;

		public FloatRange weaponMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> weaponTags;

		public FloatRange apparelMoney = FloatRange.Zero;

		public List<ThingDef> apparelRequired;

		[NoTranslate]
		public List<string> apparelTags;

		public float apparelAllowHeadgearChance = 1f;

		public bool apparelIgnoreSeasons;

		public Color apparelColor = Color.white;

		public FloatRange techHediffsMoney = FloatRange.Zero;

		[NoTranslate]
		public List<string> techHediffsTags;

		public float techHediffsChance;

		public List<ThingDefCountClass> fixedInventory = new List<ThingDefCountClass>();

		public PawnInventoryOption inventoryOptions;

		public float invNutrition;

		public ThingDef invFoodDef;

		public float chemicalAddictionChance;

		public float combatEnhancingDrugsChance;

		public IntRange combatEnhancingDrugsCount = IntRange.zero;

		public bool trader;

		[MustTranslate]
		public string labelMale;

		[MustTranslate]
		public string labelMalePlural;

		[MustTranslate]
		public string labelFemale;

		[MustTranslate]
		public string labelFemalePlural;

		public IntRange wildGroupSize = IntRange.one;

		public float ecoSystemWeight = 1f;

		[CompilerGenerated]
		private static Func<ThingDef, float> _003C_003Ef__mg_0024cache0;

		public RaceProperties RaceProps => race.race;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			for (int i = 0; i < lifeStages.Count; i++)
			{
				lifeStages[i].ResolveReferences();
			}
		}

		public string GetLabelPlural(int count = -1)
		{
			if (!labelPlural.NullOrEmpty())
			{
				return labelPlural;
			}
			return Find.ActiveLanguageWorker.Pluralize(label, count);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (race == null)
			{
				yield return "no race";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (RaceProps.Humanlike && backstoryCategories.NullOrEmpty())
			{
				yield return "Humanlike needs backstoryCategories.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (baseRecruitDifficulty > 1.0001f)
			{
				yield return defName + " recruitDifficulty is greater than 1. 1 means impossible to recruit.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (combatPower < 0f)
			{
				yield return defName + " has no combatPower.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (weaponMoney != FloatRange.Zero)
			{
				float minCost = 999999f;
				_003CConfigErrors_003Ec__Iterator0 _003CConfigErrors_003Ec__Iterator = (_003CConfigErrors_003Ec__Iterator0)/*Error near IL_01fd: stateMachine*/;
				int k;
				for (k = 0; k < weaponTags.Count; k++)
				{
					IEnumerable<ThingDef> source = from d in DefDatabase<ThingDef>.AllDefs
					where d.weaponTags != null && d.weaponTags.Contains(_003CConfigErrors_003Ec__Iterator._0024this.weaponTags[k])
					select d;
					if (source.Any())
					{
						minCost = Mathf.Min(minCost, source.Min((Func<ThingDef, float>)PawnWeaponGenerator.CheapestNonDerpPriceFor));
					}
				}
				if (minCost > weaponMoney.min)
				{
					yield return "Cheapest weapon with one of my weaponTags costs " + minCost + " but weaponMoney min is " + weaponMoney.min + ", so could end up weaponless.";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (!RaceProps.Humanlike && lifeStages.Count != RaceProps.lifeStageAges.Count)
			{
				yield return "PawnKindDef defines " + lifeStages.Count + " lifeStages while race def defines " + RaceProps.lifeStageAges.Count;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (apparelRequired != null)
			{
				for (int j = 0; j < apparelRequired.Count; j++)
				{
					for (int i = j + 1; i < apparelRequired.Count; i++)
					{
						if (!ApparelUtility.CanWearTogether(apparelRequired[j], apparelRequired[i], race.race.body))
						{
							yield return "required apparel can't be worn together (" + apparelRequired[j] + ", " + apparelRequired[i] + ")";
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_04f9:
			/*Error near IL_04fa: Unexpected return in MoveNext()*/;
		}

		public static PawnKindDef Named(string defName)
		{
			return DefDatabase<PawnKindDef>.GetNamed(defName);
		}
	}
}
