using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_Flashstorm : GameCondition
	{
		private static readonly IntRange AreaRadiusRange = new IntRange(45, 60);

		private static readonly IntRange TicksBetweenStrikes = new IntRange(320, 800);

		private const int RainDisableTicksAfterConditionEnds = 30000;

		public IntVec2 centerLocation;

		private int areaRadius;

		private int nextLightningTicks;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref centerLocation, "centerLocation");
			Scribe_Values.Look(ref areaRadius, "areaRadius", 0);
			Scribe_Values.Look(ref nextLightningTicks, "nextLightningTicks", 0);
		}

		public override void Init()
		{
			base.Init();
			areaRadius = AreaRadiusRange.RandomInRange;
			FindGoodCenterLocation();
		}

		public override void GameConditionTick()
		{
			if (Find.TickManager.TicksGame > nextLightningTicks)
			{
				Vector2 vector = Rand.UnitVector2 * Rand.Range(0f, (float)areaRadius);
				IntVec3 intVec = new IntVec3((int)Math.Round((double)vector.x) + centerLocation.x, 0, (int)Math.Round((double)vector.y) + centerLocation.z);
				if (IsGoodLocationForStrike(intVec))
				{
					base.SingleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(base.SingleMap, intVec));
					nextLightningTicks = Find.TickManager.TicksGame + TicksBetweenStrikes.RandomInRange;
				}
			}
		}

		public override void End()
		{
			base.SingleMap.weatherDecider.DisableRainFor(30000);
			base.End();
		}

		private void FindGoodCenterLocation()
		{
			IntVec3 size = base.SingleMap.Size;
			if (size.x > 16)
			{
				IntVec3 size2 = base.SingleMap.Size;
				if (size2.z > 16)
				{
					for (int i = 0; i < 10; i++)
					{
						IntVec3 size3 = base.SingleMap.Size;
						int newX = Rand.Range(8, size3.x - 8);
						IntVec3 size4 = base.SingleMap.Size;
						centerLocation = new IntVec2(newX, Rand.Range(8, size4.z - 8));
						if (IsGoodCenterLocation(centerLocation))
						{
							break;
						}
					}
					return;
				}
			}
			throw new Exception("Map too small for flashstorm.");
		}

		private bool IsGoodLocationForStrike(IntVec3 loc)
		{
			return loc.InBounds(base.SingleMap) && !loc.Roofed(base.SingleMap) && loc.Standable(base.SingleMap);
		}

		private bool IsGoodCenterLocation(IntVec2 loc)
		{
			int num = 0;
			int num2 = (int)(3.14159274f * (float)areaRadius * (float)areaRadius / 2f);
			foreach (IntVec3 potentiallyAffectedCell in GetPotentiallyAffectedCells(loc))
			{
				if (IsGoodLocationForStrike(potentiallyAffectedCell))
				{
					num++;
				}
				if (num >= num2)
				{
					break;
				}
			}
			return num >= num2;
		}

		private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
		{
			for (int x = center.x - areaRadius; x <= center.x + areaRadius; x++)
			{
				for (int z = center.z - areaRadius; z <= center.z + areaRadius; z++)
				{
					if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= areaRadius * areaRadius)
					{
						yield return new IntVec3(x, 0, z);
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
		}
	}
}
