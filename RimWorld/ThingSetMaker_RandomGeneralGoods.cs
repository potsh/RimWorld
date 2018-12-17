using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_RandomGeneralGoods : ThingSetMaker
	{
		private enum GoodsType
		{
			None,
			Meals,
			RawFood,
			Medicine,
			Drugs,
			Resources
		}

		private static Pair<GoodsType, float>[] GoodsWeights = new Pair<GoodsType, float>[5]
		{
			new Pair<GoodsType, float>(GoodsType.Meals, 1f),
			new Pair<GoodsType, float>(GoodsType.RawFood, 0.75f),
			new Pair<GoodsType, float>(GoodsType.Medicine, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Drugs, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Resources, 0.234f)
		};

		[CompilerGenerated]
		private static Func<ThingDef, bool> _003C_003Ef__mg_0024cache0;

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IntRange? countRange = parms.countRange;
			IntRange intRange = (!countRange.HasValue) ? new IntRange(10, 20) : countRange.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = techLevel.HasValue ? techLevel.Value : TechLevel.Undefined;
			int num = Mathf.Max(intRange.RandomInRange, 1);
			for (int i = 0; i < num; i++)
			{
				outThings.Add(GenerateSingle(techLevel2));
			}
		}

		private Thing GenerateSingle(TechLevel techLevel)
		{
			Thing thing = null;
			int num = 0;
			while (thing == null && num < 50)
			{
				switch (GoodsWeights.RandomElementByWeight((Pair<GoodsType, float> x) => x.Second).First)
				{
				case GoodsType.Meals:
					thing = RandomMeals(techLevel);
					break;
				case GoodsType.RawFood:
					thing = RandomRawFood(techLevel);
					break;
				case GoodsType.Medicine:
					thing = RandomMedicine(techLevel);
					break;
				case GoodsType.Drugs:
					thing = RandomDrugs(techLevel);
					break;
				case GoodsType.Resources:
					thing = RandomResources(techLevel);
					break;
				default:
					throw new Exception();
				}
				num++;
			}
			return thing;
		}

		private Thing RandomMeals(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.Pemmican;
			}
			else
			{
				float value = Rand.Value;
				thingDef = ((value < 0.5f) ? ThingDefOf.MealSimple : ((!((double)value < 0.75)) ? ThingDefOf.MealSurvivalPack : ThingDefOf.MealFine));
			}
			Thing thing = ThingMaker.MakeThing(thingDef);
			int num = Mathf.Min(thingDef.stackLimit, 10);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		private Thing RandomRawFood(TechLevel techLevel)
		{
			if (!PossibleRawFood(techLevel).TryRandomElement(out ThingDef result))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private IEnumerable<ThingDef> PossibleRawFood(TechLevel techLevel)
		{
			return from x in ThingSetMakerUtility.allGeneratableItems
			where x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.HasComp(typeof(CompHatcher)) && (int)x.techLevel <= (int)techLevel && (int)x.ingestible.preferability < 6
			select x;
		}

		private Thing RandomMedicine(TechLevel techLevel)
		{
			ThingDef result;
			if (Rand.Value < 0.75f && (int)techLevel >= (int)ThingDefOf.MedicineHerbal.techLevel)
			{
				result = (from x in ThingSetMakerUtility.allGeneratableItems
				where x.IsMedicine && (int)x.techLevel <= (int)techLevel
				select x).MaxBy((ThingDef x) => x.GetStatValueAbstract(StatDefOf.MedicalPotency));
			}
			else if (!(from x in ThingSetMakerUtility.allGeneratableItems
			where x.IsMedicine
			select x).TryRandomElement(out result))
			{
				return null;
			}
			if (techLevel.IsNeolithicOrWorse())
			{
				result = ThingDefOf.MedicineHerbal;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 20);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomDrugs(TechLevel techLevel)
		{
			if (!(from x in ThingSetMakerUtility.allGeneratableItems
			where x.IsDrug && (int)x.techLevel <= (int)techLevel
			select x).TryRandomElement(out ThingDef result))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 25);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomResources(TechLevel techLevel)
		{
			ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(techLevel);
			Thing thing = ThingMaker.MakeThing(thingDef);
			int num = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			_003CAllGeneratableThingsDebugSub_003Ec__Iterator0 _003CAllGeneratableThingsDebugSub_003Ec__Iterator = (_003CAllGeneratableThingsDebugSub_003Ec__Iterator0)/*Error near IL_0054: stateMachine*/;
			TechLevel? techLevel2 = parms.techLevel;
			TechLevel techLevel = techLevel2.HasValue ? techLevel2.Value : TechLevel.Undefined;
			if (!techLevel.IsNeolithicOrWorse())
			{
				yield return ThingDefOf.MealSimple;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return ThingDefOf.Pemmican;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
