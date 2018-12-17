using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Nutrition : ThingSetMaker
	{
		private int nextSeed;

		public ThingSetMaker_Nutrition()
		{
			nextSeed = Rand.Int;
		}

		protected override bool CanGenerateSub(ThingSetMakerParams parms)
		{
			if (!AllowedThingDefs(parms).Any())
			{
				return false;
			}
			IntRange? countRange = parms.countRange;
			if (countRange.HasValue)
			{
				IntRange value = parms.countRange.Value;
				if (value.max <= 0)
				{
					return false;
				}
			}
			FloatRange? totalNutritionRange = parms.totalNutritionRange;
			if (totalNutritionRange.HasValue)
			{
				FloatRange value2 = parms.totalNutritionRange.Value;
				if (!(value2.max <= 0f))
				{
					float? maxTotalMass = parms.maxTotalMass;
					if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
					{
						IEnumerable<ThingDef> candidates = AllowedThingDefs(parms);
						TechLevel? techLevel = parms.techLevel;
						TechLevel stuffTechLevel = techLevel.HasValue ? techLevel.Value : TechLevel.Undefined;
						float value3 = parms.maxTotalMass.Value;
						IntRange? countRange2 = parms.countRange;
						int count;
						if (countRange2.HasValue)
						{
							IntRange value4 = parms.countRange.Value;
							count = value4.min;
						}
						else
						{
							count = 1;
						}
						if (!ThingSetMakerUtility.PossibleToWeighNoMoreThan(candidates, stuffTechLevel, value3, count))
						{
							return false;
						}
					}
					if (!GeneratePossibleDefs(parms, out float _, nextSeed).Any())
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			float? maxTotalMass = parms.maxTotalMass;
			float maxMass = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
			float totalNutrition;
			List<ThingStuffPairWithQuality> list = GeneratePossibleDefs(parms, out totalNutrition, nextSeed);
			for (int i = 0; i < list.Count; i++)
			{
				outThings.Add(list[i].MakeThing());
			}
			ThingSetMakerByTotalStatUtility.IncreaseStackCountsToTotalValue(outThings, totalNutrition, (Thing x) => x.GetStatValue(StatDefOf.Nutrition), maxMass);
			nextSeed++;
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition, int seed)
		{
			Rand.PushState(seed);
			List<ThingStuffPairWithQuality> result = GeneratePossibleDefs(parms, out totalNutrition);
			Rand.PopState();
			return result;
		}

		private List<ThingStuffPairWithQuality> GeneratePossibleDefs(ThingSetMakerParams parms, out float totalNutrition)
		{
			IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
			if (!enumerable.Any())
			{
				totalNutrition = 0f;
				return new List<ThingStuffPairWithQuality>();
			}
			IntRange? countRange = parms.countRange;
			IntRange intRange = (!countRange.HasValue) ? new IntRange(1, 2147483647) : countRange.Value;
			FloatRange? totalNutritionRange = parms.totalNutritionRange;
			FloatRange floatRange = (!totalNutritionRange.HasValue) ? FloatRange.Zero : totalNutritionRange.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = techLevel.HasValue ? techLevel.Value : TechLevel.Undefined;
			float? maxTotalMass = parms.maxTotalMass;
			float num = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
			QualityGenerator? qualityGenerator = parms.qualityGenerator;
			QualityGenerator qualityGenerator2 = qualityGenerator.HasValue ? qualityGenerator.Value : QualityGenerator.BaseGen;
			totalNutrition = floatRange.RandomInRange;
			int numMeats = enumerable.Count((ThingDef x) => x.IsMeat);
			int numLeathers = enumerable.Count((ThingDef x) => x.IsLeather);
			Func<ThingDef, float> func = (ThingDef x) => ThingSetMakerUtility.AdjustedBigCategoriesSelectionWeight(x, numMeats, numLeathers);
			IntRange countRange2 = intRange;
			float totalValue = totalNutrition;
			IEnumerable<ThingDef> allowed = enumerable;
			TechLevel techLevel3 = techLevel2;
			QualityGenerator qualityGenerator3 = qualityGenerator2;
			Func<ThingStuffPairWithQuality, float> getMinValue = (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition);
			Func<ThingStuffPairWithQuality, float> getMaxValue = (ThingStuffPairWithQuality x) => x.GetStatValue(StatDefOf.Nutrition) * (float)x.thing.stackLimit;
			Func<ThingDef, float> weightSelector = func;
			float maxMass = num;
			return ThingSetMakerByTotalStatUtility.GenerateDefsWithPossibleTotalValue(countRange2, totalValue, allowed, techLevel3, qualityGenerator3, getMinValue, getMaxValue, weightSelector, 100, maxMass);
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel? techLevel2 = parms.techLevel;
			TechLevel techLevel = techLevel2.HasValue ? techLevel2.Value : TechLevel.Undefined;
			foreach (ThingDef item in AllowedThingDefs(parms))
			{
				float? maxTotalMass = parms.maxTotalMass;
				if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
				{
					float? maxTotalMass2 = parms.maxTotalMass;
					if (maxTotalMass2.HasValue && ThingSetMakerUtility.GetMinMass(item, techLevel) > maxTotalMass2.GetValueOrDefault())
					{
						continue;
					}
				}
				FloatRange? totalNutritionRange = parms.totalNutritionRange;
				if (totalNutritionRange.HasValue)
				{
					FloatRange value = parms.totalNutritionRange.Value;
					if (value.max != 3.40282347E+38f && item.IsNutritionGivingIngestible)
					{
						float cachedNutrition = item.ingestible.CachedNutrition;
						FloatRange value2 = parms.totalNutritionRange.Value;
						if (cachedNutrition > value2.max)
						{
							continue;
						}
					}
				}
				yield return item;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01ee:
			/*Error near IL_01ef: Unexpected return in MoveNext()*/;
		}
	}
}
