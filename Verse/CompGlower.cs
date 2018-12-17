using RimWorld;

namespace Verse
{
	public class CompGlower : ThingComp
	{
		private bool glowOnInt;

		public CompProperties_Glower Props => (CompProperties_Glower)props;

		private bool ShouldBeLitNow
		{
			get
			{
				if (!parent.Spawned)
				{
					return false;
				}
				if (!FlickUtility.WantsToBeOn(parent))
				{
					return false;
				}
				CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
				if (compPowerTrader != null && !compPowerTrader.PowerOn)
				{
					return false;
				}
				CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
				if (compRefuelable != null && !compRefuelable.HasFuel)
				{
					return false;
				}
				return true;
			}
		}

		public void UpdateLit(Map map)
		{
			bool shouldBeLitNow = ShouldBeLitNow;
			if (glowOnInt != shouldBeLitNow)
			{
				glowOnInt = shouldBeLitNow;
				if (!glowOnInt)
				{
					map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					map.glowGrid.DeRegisterGlower(this);
				}
				else
				{
					map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
					map.glowGrid.RegisterGlower(this);
				}
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (ShouldBeLitNow)
			{
				UpdateLit(parent.Map);
				parent.Map.glowGrid.RegisterGlower(this);
			}
			else
			{
				UpdateLit(parent.Map);
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			if (signal == "PowerTurnedOn" || signal == "PowerTurnedOff" || signal == "FlickedOn" || signal == "FlickedOff" || signal == "Refueled" || signal == "RanOutOfFuel" || signal == "ScheduledOn" || signal == "ScheduledOff")
			{
				UpdateLit(parent.Map);
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref glowOnInt, "glowOn", defaultValue: false);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			UpdateLit(map);
		}
	}
}
