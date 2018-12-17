using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompRefuelable : ThingComp
	{
		private float fuel;

		private float configuredTargetFuelLevel = -1f;

		private CompFlickable flickComp;

		public const string RefueledSignal = "Refueled";

		public const string RanOutOfFuelSignal = "RanOutOfFuel";

		private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel");

		private static readonly Vector2 FuelBarSize = new Vector2(1f, 0.2f);

		private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));

		private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

		public float TargetFuelLevel
		{
			get
			{
				if (configuredTargetFuelLevel >= 0f)
				{
					return configuredTargetFuelLevel;
				}
				if (Props.targetFuelLevelConfigurable)
				{
					return Props.initialConfigurableTargetFuelLevel;
				}
				return Props.fuelCapacity;
			}
			set
			{
				configuredTargetFuelLevel = Mathf.Clamp(value, 0f, Props.fuelCapacity);
			}
		}

		public CompProperties_Refuelable Props => (CompProperties_Refuelable)props;

		public float Fuel => fuel;

		public float FuelPercentOfTarget => fuel / TargetFuelLevel;

		public float FuelPercentOfMax => fuel / Props.fuelCapacity;

		public bool IsFull => TargetFuelLevel - fuel < 1f;

		public bool HasFuel => fuel > 0f && fuel >= Props.minimumFueledThreshold;

		private float ConsumptionRatePerTick => Props.fuelConsumptionRate / 60000f;

		public bool ShouldAutoRefuelNow => FuelPercentOfTarget <= Props.autoRefuelPercent && !IsFull && TargetFuelLevel > 0f && ShouldAutoRefuelNowIgnoringFuelPct;

		public bool ShouldAutoRefuelNowIgnoringFuelPct => !parent.IsBurning() && (flickComp == null || flickComp.SwitchIsOn) && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null && parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			fuel = Props.fuelCapacity * Props.initialFuelPercent;
			flickComp = parent.GetComp<CompFlickable>();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref fuel, "fuel", 0f);
			Scribe_Values.Look(ref configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f);
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!HasFuel && Props.drawOutOfFuelOverlay)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.OutOfFuel);
			}
			if (Props.drawFuelGaugeInMap)
			{
				GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
				r.center = parent.DrawPos + Vector3.up * 0.1f;
				r.size = FuelBarSize;
				r.fillPercent = FuelPercentOfMax;
				r.filledMat = FuelBarFilledMat;
				r.unfilledMat = FuelBarUnfilledMat;
				r.margin = 0.15f;
				Rot4 rotation = parent.Rotation;
				rotation.Rotate(RotationDirection.Clockwise);
				r.rotation = rotation;
				GenDraw.DrawFillableBar(r);
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (previousMap != null && Props.fuelFilter.AllowedDefCount == 1 && Props.initialFuelPercent == 0f)
			{
				ThingDef thingDef = Props.fuelFilter.AllowedThingDefs.First();
				float num = 1f;
				int num2 = GenMath.RoundRandom(num * fuel);
				while (num2 > 0)
				{
					Thing thing = ThingMaker.MakeThing(thingDef);
					thing.stackCount = Mathf.Min(num2, thingDef.stackLimit);
					num2 -= thing.stackCount;
					GenPlace.TryPlaceThing(thing, parent.Position, previousMap, ThingPlaceMode.Near);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = Props.FuelLabel + ": " + fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
			if (!Props.consumeFuelOnlyWhenUsed && HasFuel)
			{
				int numTicks = (int)(fuel / Props.fuelConsumptionRate * 60000f);
				text = text + " (" + numTicks.ToStringTicksToPeriod() + ")";
			}
			if (!HasFuel && !Props.outOfFuelMessage.NullOrEmpty())
			{
				text += $"\n{Props.outOfFuelMessage} ({GetFuelCountToFullyRefuel()}x {Props.fuelFilter.AnyAllowedDef.label})";
			}
			if (Props.targetFuelLevelConfigurable)
			{
				text = text + "\n" + "ConfiguredTargetFuelLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall());
			}
			return text;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (!Props.consumeFuelOnlyWhenUsed && (flickComp == null || flickComp.SwitchIsOn))
			{
				ConsumeFuel(ConsumptionRatePerTick);
			}
			if (Props.fuelConsumptionPerTickInRain > 0f && parent.Spawned && parent.Map.weatherManager.RainRate > 0.4f && !parent.Map.roofGrid.Roofed(parent.Position))
			{
				ConsumeFuel(Props.fuelConsumptionPerTickInRain);
			}
		}

		public void ConsumeFuel(float amount)
		{
			if (!(fuel <= 0f))
			{
				fuel -= amount;
				if (fuel <= 0f)
				{
					fuel = 0f;
					if (Props.destroyOnNoFuel)
					{
						parent.Destroy();
					}
					parent.BroadcastCompSignal("RanOutOfFuel");
				}
			}
		}

		public void Refuel(List<Thing> fuelThings)
		{
			if (Props.atomicFueling && fuelThings.Sum((Thing t) => t.stackCount) < GetFuelCountToFullyRefuel())
			{
				Log.ErrorOnce("Error refueling; not enough fuel available for proper atomic refuel", 19586442);
			}
			else
			{
				int num = GetFuelCountToFullyRefuel();
				while (num > 0 && fuelThings.Count > 0)
				{
					Thing thing = fuelThings.Pop();
					int num2 = Mathf.Min(num, thing.stackCount);
					Refuel((float)num2);
					thing.SplitOff(num2).Destroy();
					num -= num2;
				}
			}
		}

		public void Refuel(float amount)
		{
			fuel += amount * Props.FuelMultiplierCurrentDifficulty;
			if (fuel > Props.fuelCapacity)
			{
				fuel = Props.fuelCapacity;
			}
			parent.BroadcastCompSignal("Refueled");
		}

		public void Notify_UsedThisTick()
		{
			ConsumeFuel(ConsumptionRatePerTick);
		}

		public int GetFuelCountToFullyRefuel()
		{
			if (Props.atomicFueling)
			{
				return Mathf.CeilToInt(Props.fuelCapacity / Props.FuelMultiplierCurrentDifficulty);
			}
			float f = (TargetFuelLevel - fuel) / Props.FuelMultiplierCurrentDifficulty;
			return Mathf.Max(Mathf.CeilToInt(f), 1);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Props.targetFuelLevelConfigurable)
			{
				yield return (Gizmo)new Command_SetTargetFuelLevel
				{
					refuelable = this,
					defaultLabel = "CommandSetTargetFuelLevel".Translate(),
					defaultDesc = "CommandSetTargetFuelLevelDesc".Translate(),
					icon = SetTargetFuelLevelCommand
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Props.showFuelGizmo && Find.Selector.SingleSelectedThing == parent)
			{
				yield return (Gizmo)new Gizmo_RefuelableFuelStatus
				{
					refuelable = this
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "Debug: Set fuel to 0",
					action = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0152: stateMachine*/)._0024this.fuel = 0f;
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0152: stateMachine*/)._0024this.parent.BroadcastCompSignal("Refueled");
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
