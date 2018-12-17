using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSetMakerUtility
	{
		public static List<ThingDef> allGeneratableItems = new List<ThingDef>();

		public static void Reset()
		{
			allGeneratableItems.Clear();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (CanGenerate(allDef))
				{
					allGeneratableItems.Add(allDef);
				}
			}
			ThingSetMaker_Meteorite.Reset();
		}

		public static bool CanGenerate(ThingDef thingDef)
		{
			if ((thingDef.category != ThingCategory.Item && !thingDef.Minifiable) || (thingDef.category == ThingCategory.Item && !thingDef.EverHaulable) || thingDef.isUnfinishedThing || thingDef.IsCorpse || !thingDef.PlayerAcquirable || thingDef.graphicData == null || typeof(MinifiedThing).IsAssignableFrom(thingDef.thingClass))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<ThingDef> GetAllowedThingDefs(ThingSetMakerParams parms)
		{
			TechLevel? techLevel2 = parms.techLevel;
			TechLevel techLevel = techLevel2.HasValue ? techLevel2.Value : TechLevel.Undefined;
			IEnumerable<ThingDef> source = parms.filter.AllowedThingDefs;
			if (techLevel != 0)
			{
				source = from x in source
				where (int)x.techLevel <= (int)techLevel
				select x;
			}
			return source.Where(delegate(ThingDef x)
			{
				if (!CanGenerate(x))
				{
					goto IL_007e;
				}
				float? maxThingMarketValue = parms.maxThingMarketValue;
				if (maxThingMarketValue.HasValue)
				{
					float? maxThingMarketValue2 = parms.maxThingMarketValue;
					if (!maxThingMarketValue2.HasValue || !(x.BaseMarketValue <= maxThingMarketValue2.GetValueOrDefault()))
					{
						goto IL_007e;
					}
				}
				int result = (parms.validator == null || parms.validator(x)) ? 1 : 0;
				goto IL_007f;
				IL_007e:
				result = 0;
				goto IL_007f;
				IL_007f:
				return (byte)result != 0;
			});
		}

		public static void AssignQuality(Thing thing, QualityGenerator? qualityGenerator)
		{
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				QualityGenerator qualityGenerator2 = qualityGenerator.HasValue ? qualityGenerator.Value : QualityGenerator.BaseGen;
				QualityCategory q = QualityUtility.GenerateQuality(qualityGenerator2);
				compQuality.SetQuality(q, ArtGenerationContext.Outsider);
			}
		}

		public static float AdjustedBigCategoriesSelectionWeight(ThingDef d, int numMeats, int numLeathers)
		{
			float num = 1f;
			if (d.IsMeat)
			{
				num *= Mathf.Min(5f / (float)numMeats, 1f);
			}
			if (d.IsLeather)
			{
				num *= Mathf.Min(5f / (float)numLeathers, 1f);
			}
			return num;
		}

		public static bool PossibleToWeighNoMoreThan(ThingDef t, float maxMass, IEnumerable<ThingDef> allowedStuff)
		{
			if (maxMass == 3.40282347E+38f || t.category == ThingCategory.Pawn)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			if (t.MadeFromStuff)
			{
				foreach (ThingDef item in allowedStuff)
				{
					if (t.GetStatValueAbstract(StatDefOf.Mass, item) <= maxMass)
					{
						return true;
					}
				}
				return false;
			}
			return t.GetStatValueAbstract(StatDefOf.Mass) <= maxMass;
		}

		public static bool TryGetRandomThingWhichCanWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, out ThingStuffPair thingStuffPair)
		{
			if (!(from x in candidates
			where PossibleToWeighNoMoreThan(x, maxMass, GenStuff.AllowedStuffsFor(x, stuffTechLevel))
			select x).TryRandomElement(out ThingDef thingDef))
			{
				thingStuffPair = default(ThingStuffPair);
				return false;
			}
			ThingDef result;
			if (thingDef.MadeFromStuff)
			{
				if (!(from x in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel)
				where thingDef.GetStatValueAbstract(StatDefOf.Mass, x) <= maxMass
				select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out result))
				{
					thingStuffPair = default(ThingStuffPair);
					return false;
				}
			}
			else
			{
				result = null;
			}
			thingStuffPair = new ThingStuffPair(thingDef, result);
			return true;
		}

		public static bool PossibleToWeighNoMoreThan(IEnumerable<ThingDef> candidates, TechLevel stuffTechLevel, float maxMass, int count)
		{
			if (maxMass == 3.40282347E+38f || count <= 0)
			{
				return true;
			}
			if (maxMass < 0f)
			{
				return false;
			}
			float num = 3.40282347E+38f;
			foreach (ThingDef candidate in candidates)
			{
				num = Mathf.Min(num, GetMinMass(candidate, stuffTechLevel));
			}
			return num <= maxMass * (float)count;
		}

		public static float GetMinMass(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = 3.40282347E+38f;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (item.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass, item));
					}
				}
				return num;
			}
			return Mathf.Min(num, thingDef.GetStatValueAbstract(StatDefOf.Mass));
		}

		public static float GetMinMarketValue(ThingDef thingDef, TechLevel stuffTechLevel)
		{
			float num = 3.40282347E+38f;
			if (thingDef.MadeFromStuff)
			{
				foreach (ThingDef item in GenStuff.AllowedStuffsFor(thingDef, stuffTechLevel))
				{
					if (item.stuffProps.commonality > 0f)
					{
						num = Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, item, QualityCategory.Awful)));
					}
				}
				return num;
			}
			return Mathf.Min(num, StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(thingDef, null, QualityCategory.Awful)));
		}
	}
}
