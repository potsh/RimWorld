using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_StackCount : ThingSetMaker
	{
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
			float? maxTotalMass = parms.maxTotalMass;
			if (maxTotalMass.HasValue && parms.maxTotalMass != 3.40282347E+38f)
			{
				IEnumerable<ThingDef> candidates = AllowedThingDefs(parms);
				TechLevel? techLevel = parms.techLevel;
				TechLevel stuffTechLevel = techLevel.HasValue ? techLevel.Value : TechLevel.Undefined;
				float value2 = parms.maxTotalMass.Value;
				IntRange? countRange2 = parms.countRange;
				int count;
				if (countRange2.HasValue)
				{
					IntRange value3 = parms.countRange.Value;
					count = value3.max;
				}
				else
				{
					count = 1;
				}
				if (!ThingSetMakerUtility.PossibleToWeighNoMoreThan(candidates, stuffTechLevel, value2, count))
				{
					return false;
				}
			}
			return true;
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
			if (enumerable.Any())
			{
				TechLevel? techLevel = parms.techLevel;
				TechLevel stuffTechLevel = techLevel.HasValue ? techLevel.Value : TechLevel.Undefined;
				IntRange? countRange = parms.countRange;
				IntRange intRange = (!countRange.HasValue) ? IntRange.one : countRange.Value;
				float? maxTotalMass = parms.maxTotalMass;
				float num = (!maxTotalMass.HasValue) ? 3.40282347E+38f : maxTotalMass.Value;
				int num2 = Mathf.Max(intRange.RandomInRange, 1);
				float num3 = 0f;
				int num4 = num2;
				ThingStuffPair thingStuffPair;
				while (num4 > 0 && ThingSetMakerUtility.TryGetRandomThingWhichCanWeighNoMoreThan(enumerable, stuffTechLevel, (num != 3.40282347E+38f) ? (num - num3) : 3.40282347E+38f, out thingStuffPair))
				{
					Thing thing = ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
					ThingSetMakerUtility.AssignQuality(thing, parms.qualityGenerator);
					int num5 = num4;
					if (num != 3.40282347E+38f && !(thing is Pawn))
					{
						num5 = Mathf.Min(num5, Mathf.FloorToInt((num - num3) / thing.GetStatValue(StatDefOf.Mass)));
					}
					num4 -= (thing.stackCount = Mathf.Clamp(num5, 1, thing.def.stackLimit));
					outThings.Add(thing);
					if (!(thing is Pawn))
					{
						num3 += thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount;
					}
				}
			}
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			return ThingSetMakerUtility.GetAllowedThingDefs(parms);
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
				yield return item;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_016f:
			/*Error near IL_0170: Unexpected return in MoveNext()*/;
		}
	}
}
