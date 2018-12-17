using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingSetMakerByTotalStatUtility
	{
		private static List<ThingStuffPairWithQuality> allowedThingStuffPairs = new List<ThingStuffPairWithQuality>();

		public static List<ThingStuffPairWithQuality> GenerateDefsWithPossibleTotalValue(IntRange countRange, float totalValue, IEnumerable<ThingDef> allowed, TechLevel techLevel, QualityGenerator qualityGenerator, Func<ThingStuffPairWithQuality, float> getMinValue, Func<ThingStuffPairWithQuality, float> getMaxValue, Func<ThingDef, float> weightSelector = null, int tries = 100, float maxMass = float.MaxValue)
		{
			List<ThingStuffPairWithQuality> chosen = new List<ThingStuffPairWithQuality>();
			if (countRange.max <= 0)
			{
				return chosen;
			}
			if (countRange.min < 1)
			{
				countRange.min = 1;
			}
			CalculateAllowedThingStuffPairs(allowed, techLevel, qualityGenerator);
			float trashThreshold = GetTrashThreshold(countRange, totalValue, getMaxValue);
			allowedThingStuffPairs.RemoveAll((ThingStuffPairWithQuality x) => getMaxValue(x) < trashThreshold);
			if (!allowedThingStuffPairs.Any())
			{
				return chosen;
			}
			float minCandidateValueEver = 3.40282347E+38f;
			float maxCandidateValueEver = -3.40282347E+38f;
			float minMassEver = 3.40282347E+38f;
			foreach (ThingStuffPairWithQuality allowedThingStuffPair in allowedThingStuffPairs)
			{
				ThingStuffPairWithQuality current = allowedThingStuffPair;
				minCandidateValueEver = Mathf.Min(minCandidateValueEver, getMinValue(current));
				maxCandidateValueEver = Mathf.Max(maxCandidateValueEver, getMaxValue(current));
				if (current.thing.category != ThingCategory.Pawn)
				{
					minMassEver = Mathf.Min(minMassEver, GetNonTrashMass(current, trashThreshold, getMinValue));
				}
			}
			minCandidateValueEver = Mathf.Max(minCandidateValueEver, trashThreshold);
			float totalMinValueSoFar = 0f;
			float totalMaxValueSoFar = 0f;
			float minMassSoFar = 0f;
			int num = 0;
			do
			{
				num++;
				if (num > 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				IEnumerable<ThingStuffPairWithQuality> enumerable = allowedThingStuffPairs.Where(delegate(ThingStuffPairWithQuality x)
				{
					if (maxMass != 3.40282347E+38f && x.thing.category != ThingCategory.Pawn)
					{
						float nonTrashMass = GetNonTrashMass(x, trashThreshold, getMinValue);
						if (minMassSoFar + nonTrashMass > maxMass)
						{
							return false;
						}
						if (chosen.Count < countRange.min && minMassSoFar + minMassEver * (float)(countRange.min - chosen.Count - 1) + nonTrashMass > maxMass)
						{
							return false;
						}
					}
					if (totalMinValueSoFar + Mathf.Max(getMinValue(x), trashThreshold) > totalValue)
					{
						return false;
					}
					if (chosen.Count < countRange.min && totalMinValueSoFar + minCandidateValueEver * (float)(countRange.min - chosen.Count - 1) + Mathf.Max(getMinValue(x), trashThreshold) > totalValue)
					{
						return false;
					}
					return true;
				});
				if (countRange.max != 2147483647 && totalMaxValueSoFar < totalValue * 0.5f)
				{
					IEnumerable<ThingStuffPairWithQuality> enumerable2 = enumerable;
					enumerable = from x in enumerable
					where totalMaxValueSoFar + maxCandidateValueEver * (float)(countRange.max - chosen.Count - 1) + getMaxValue(x) >= totalValue * 0.5f
					select x;
					if (!enumerable.Any())
					{
						enumerable = enumerable2;
					}
				}
				float maxCandidateMinValue = -3.40282347E+38f;
				foreach (ThingStuffPairWithQuality item in enumerable)
				{
					maxCandidateMinValue = Mathf.Max(maxCandidateMinValue, Mathf.Max(getMinValue(item), trashThreshold));
				}
				if (!enumerable.TryRandomElementByWeight(delegate(ThingStuffPairWithQuality x)
				{
					float a = 1f;
					if (countRange.max != 2147483647 && chosen.Count < countRange.max && totalValue >= totalMaxValueSoFar)
					{
						int num2 = countRange.max - chosen.Count;
						float b = (totalValue - totalMaxValueSoFar) / (float)num2;
						a = Mathf.InverseLerp(0f, b, getMaxValue(x));
					}
					float b2 = 1f;
					if (chosen.Count < countRange.min && totalValue >= totalMinValueSoFar)
					{
						int num3 = countRange.min - chosen.Count;
						float num4 = (totalValue - totalMinValueSoFar) / (float)num3;
						float num5 = Mathf.Max(getMinValue(x), trashThreshold);
						if (num5 > num4)
						{
							b2 = num4 / num5;
						}
					}
					float num6 = Mathf.Max(Mathf.Min(a, b2), 1E-05f);
					if (weightSelector != null)
					{
						num6 *= weightSelector(x.thing);
					}
					if (totalValue > totalMaxValueSoFar)
					{
						int num7 = Mathf.Max(countRange.min - chosen.Count, 1);
						float num8 = Mathf.InverseLerp(0f, maxCandidateMinValue * 0.85f, GetMaxValueWithMaxMass(x, minMassSoFar, maxMass, getMinValue, getMaxValue) * (float)num7);
						num6 *= num8 * num8;
					}
					if (PawnWeaponGenerator.IsDerpWeapon(x.thing, x.stuff))
					{
						num6 *= 0.1f;
					}
					if (techLevel != 0)
					{
						TechLevel techLevel2 = x.thing.techLevel;
						if ((int)techLevel2 < (int)techLevel && (int)techLevel2 <= 2 && (x.thing.IsApparel || x.thing.IsWeapon))
						{
							num6 *= 0.1f;
						}
					}
					return num6;
				}, out ThingStuffPairWithQuality result))
				{
					break;
				}
				chosen.Add(result);
				totalMinValueSoFar += Mathf.Max(getMinValue(result), trashThreshold);
				totalMaxValueSoFar += getMaxValue(result);
				if (result.thing.category != ThingCategory.Pawn)
				{
					minMassSoFar += GetNonTrashMass(result, trashThreshold, getMinValue);
				}
			}
			while (chosen.Count < countRange.max && (chosen.Count < countRange.min || !(totalMaxValueSoFar >= totalValue * 0.9f)));
			return chosen;
		}

		public static void IncreaseStackCountsToTotalValue(List<Thing> things, float totalValue, Func<Thing, float> getValue, float maxMass = float.MaxValue)
		{
			float currentTotalValue = 0f;
			float currentTotalMass = 0f;
			for (int i = 0; i < things.Count; i++)
			{
				currentTotalValue += getValue(things[i]) * (float)things[i].stackCount;
				if (!(things[i] is Pawn))
				{
					currentTotalMass += things[i].GetStatValue(StatDefOf.Mass) * (float)things[i].stackCount;
				}
			}
			if (!(currentTotalValue >= totalValue) && !(currentTotalMass >= maxMass))
			{
				things.SortByDescending((Thing x) => getValue(x) / x.GetStatValue(StatDefOf.Mass));
				DistributeEvenly(things, currentTotalValue + (totalValue - currentTotalValue) * 0.1f, ref currentTotalValue, ref currentTotalMass, getValue, (maxMass != 3.40282347E+38f) ? (currentTotalMass + (maxMass - currentTotalMass) * 0.1f) : 3.40282347E+38f);
				if (!(currentTotalValue >= totalValue) && !(currentTotalMass >= maxMass))
				{
					DistributeEvenly(things, totalValue, ref currentTotalValue, ref currentTotalMass, getValue, maxMass, useValueMassRatio: true);
					if (!(currentTotalValue >= totalValue) && !(currentTotalMass >= maxMass))
					{
						GiveRemainingValueToAnything(things, totalValue, ref currentTotalValue, ref currentTotalMass, getValue, maxMass);
					}
				}
			}
		}

		private static void DistributeEvenly(List<Thing> things, float totalValue, ref float currentTotalValue, ref float currentTotalMass, Func<Thing, float> getValue, float maxMass, bool useValueMassRatio = false)
		{
			float num = (totalValue - currentTotalValue) / (float)things.Count;
			float num2 = maxMass - currentTotalMass;
			float num3 = (maxMass != 3.40282347E+38f) ? (num2 / (float)things.Count) : 3.40282347E+38f;
			float num4 = 0f;
			if (useValueMassRatio)
			{
				for (int i = 0; i < things.Count; i++)
				{
					num4 += getValue(things[i]) / things[i].GetStatValue(StatDefOf.Mass);
				}
			}
			for (int j = 0; j < things.Count; j++)
			{
				float num5 = getValue(things[j]);
				int a = Mathf.FloorToInt(num / num5);
				int num6 = Mathf.Min(a, things[j].def.stackLimit - things[j].stackCount);
				if (maxMass != 3.40282347E+38f && !(things[j] is Pawn))
				{
					num6 = Mathf.Min(num6, Mathf.FloorToInt(Mathf.Min(b: (!useValueMassRatio) ? num3 : (num2 * (getValue(things[j]) / things[j].GetStatValue(StatDefOf.Mass) / num4)), a: maxMass - currentTotalMass) / things[j].GetStatValue(StatDefOf.Mass)));
				}
				if (num6 > 0)
				{
					things[j].stackCount += num6;
					currentTotalValue += num5 * (float)num6;
					if (!(things[j] is Pawn))
					{
						currentTotalMass += things[j].GetStatValue(StatDefOf.Mass) * (float)num6;
					}
				}
			}
		}

		private static void GiveRemainingValueToAnything(List<Thing> things, float totalValue, ref float currentTotalValue, ref float currentTotalMass, Func<Thing, float> getValue, float maxMass)
		{
			for (int i = 0; i < things.Count; i++)
			{
				float num = getValue(things[i]);
				int a = Mathf.FloorToInt((totalValue - currentTotalValue) / num);
				int num2 = Mathf.Min(a, things[i].def.stackLimit - things[i].stackCount);
				if (maxMass != 3.40282347E+38f && !(things[i] is Pawn))
				{
					num2 = Mathf.Min(num2, Mathf.FloorToInt((maxMass - currentTotalMass) / things[i].GetStatValue(StatDefOf.Mass)));
				}
				if (num2 > 0)
				{
					things[i].stackCount += num2;
					currentTotalValue += num * (float)num2;
					if (!(things[i] is Pawn))
					{
						currentTotalMass += things[i].GetStatValue(StatDefOf.Mass) * (float)num2;
					}
				}
			}
		}

		private static void CalculateAllowedThingStuffPairs(IEnumerable<ThingDef> allowed, TechLevel techLevel, QualityGenerator qualityGenerator)
		{
			allowedThingStuffPairs.Clear();
			foreach (ThingDef item in allowed)
			{
				for (int i = 0; i < 5; i++)
				{
					if (GenStuff.TryRandomStuffFor(item, out ThingDef stuff, techLevel))
					{
						QualityCategory quality = (!item.HasComp(typeof(CompQuality))) ? QualityCategory.Normal : QualityUtility.GenerateQuality(qualityGenerator);
						allowedThingStuffPairs.Add(new ThingStuffPairWithQuality(item, stuff, quality));
					}
				}
			}
		}

		private static float GetTrashThreshold(IntRange countRange, float totalValue, Func<ThingStuffPairWithQuality, float> getMaxValue)
		{
			float num = GenMath.Median(allowedThingStuffPairs, getMaxValue);
			int num2 = Mathf.Clamp(Mathf.CeilToInt(totalValue / num), countRange.min, countRange.max);
			return totalValue / (float)num2 * 0.2f;
		}

		private static float GetNonTrashMass(ThingStuffPairWithQuality t, float trashThreshold, Func<ThingStuffPairWithQuality, float> getMinValue)
		{
			int num = Mathf.Clamp(Mathf.CeilToInt(trashThreshold / getMinValue(t)), 1, t.thing.stackLimit);
			return t.GetStatValue(StatDefOf.Mass) * (float)num;
		}

		private static float GetMaxValueWithMaxMass(ThingStuffPairWithQuality t, float massSoFar, float maxMass, Func<ThingStuffPairWithQuality, float> getMinValue, Func<ThingStuffPairWithQuality, float> getMaxValue)
		{
			if (maxMass == 3.40282347E+38f)
			{
				return getMaxValue(t);
			}
			int num = Mathf.Clamp(Mathf.FloorToInt((maxMass - massSoFar) / t.GetStatValue(StatDefOf.Mass)), 1, t.thing.stackLimit);
			return getMinValue(t) * (float)num;
		}
	}
}
