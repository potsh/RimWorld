using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_Refuelable : CompProperties
	{
		public float fuelConsumptionRate = 1f;

		public float fuelCapacity = 2f;

		public float initialFuelPercent;

		public float autoRefuelPercent = 0.3f;

		public float fuelConsumptionPerTickInRain;

		public ThingFilter fuelFilter;

		public bool destroyOnNoFuel;

		public bool consumeFuelOnlyWhenUsed;

		public bool showFuelGizmo;

		public bool targetFuelLevelConfigurable;

		public float initialConfigurableTargetFuelLevel;

		public bool drawOutOfFuelOverlay = true;

		public float minimumFueledThreshold;

		public bool drawFuelGaugeInMap;

		public bool atomicFueling;

		private float fuelMultiplier = 1f;

		public bool factorByDifficulty;

		public string fuelLabel;

		public string fuelGizmoLabel;

		public string outOfFuelMessage;

		public string fuelIconPath;

		private Texture2D fuelIcon;

		public string FuelLabel => fuelLabel.NullOrEmpty() ? "Fuel".Translate() : fuelLabel;

		public string FuelGizmoLabel => fuelGizmoLabel.NullOrEmpty() ? "Fuel".Translate() : fuelGizmoLabel;

		public Texture2D FuelIcon
		{
			get
			{
				if (fuelIcon == null)
				{
					if (!fuelIconPath.NullOrEmpty())
					{
						fuelIcon = ContentFinder<Texture2D>.Get(fuelIconPath);
					}
					else
					{
						ThingDef thingDef = (fuelFilter.AnyAllowedDef == null) ? ThingDefOf.Chemfuel : fuelFilter.AnyAllowedDef;
						fuelIcon = thingDef.uiIcon;
					}
				}
				return fuelIcon;
			}
		}

		public float FuelMultiplierCurrentDifficulty
		{
			get
			{
				if (factorByDifficulty)
				{
					return fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
				}
				return fuelMultiplier;
			}
		}

		public CompProperties_Refuelable()
		{
			compClass = typeof(CompRefuelable);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			fuelFilter.ResolveReferences();
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (destroyOnNoFuel && initialFuelPercent <= 0f)
			{
				yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if ((!consumeFuelOnlyWhenUsed || fuelConsumptionPerTickInRain > 0f) && parentDef.tickerType != TickerType.Normal)
			{
				yield return $"Refuelable component set to consume fuel per tick, but parent tickertype is {parentDef.tickerType} instead of {TickerType.Normal}";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_017b:
			/*Error near IL_017c: Unexpected return in MoveNext()*/;
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
			if (((ThingDef)req.Def).building.IsTurret)
			{
				StatCategoryDef building = StatCategoryDefOf.Building;
				string label = "RearmCost".Translate();
				string valueString = GenLabel.ThingLabel(fuelFilter.AnyAllowedDef, null, (int)(fuelCapacity / FuelMultiplierCurrentDifficulty)).CapitalizeFirst();
				string overrideReportText = "RearmCostExplanation".Translate();
				yield return new StatDrawEntry(building, label, valueString, 0, overrideReportText);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01be:
			/*Error near IL_01bf: Unexpected return in MoveNext()*/;
		}
	}
}
