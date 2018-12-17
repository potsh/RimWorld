using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompTerrainPump : ThingComp
	{
		private CompPowerTrader powerComp;

		private int progressTicks;

		private CompProperties_TerrainPump Props => (CompProperties_TerrainPump)props;

		private float ProgressDays => (float)progressTicks / 60000f;

		private float CurrentRadius => Mathf.Min(Props.radius, ProgressDays / Props.daysToRadius * Props.radius);

		private bool Working => powerComp == null || powerComp.PowerOn;

		private int TicksUntilRadiusInteger
		{
			get
			{
				float num = Mathf.Ceil(CurrentRadius) - CurrentRadius;
				if (num < 1E-05f)
				{
					num = 1f;
				}
				float num2 = Props.radius / Props.daysToRadius;
				float num3 = num / num2;
				return (int)(num3 * 60000f);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			powerComp = parent.TryGetComp<CompPowerTrader>();
		}

		public override void PostDeSpawn(Map map)
		{
			progressTicks = 0;
		}

		public override void CompTickRare()
		{
			if (Working)
			{
				progressTicks += 250;
				int num = GenRadial.NumCellsInRadius(CurrentRadius);
				for (int i = 0; i < num; i++)
				{
					AffectCell(parent.Position + GenRadial.RadialPattern[i]);
				}
			}
		}

		protected abstract void AffectCell(IntVec3 c);

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref progressTicks, "progressTicks", 0);
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if (CurrentRadius < Props.radius - 0.0001f)
			{
				GenDraw.DrawRadiusRing(parent.Position, CurrentRadius);
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = "TimePassed".Translate().CapitalizeFirst() + ": " + progressTicks.ToStringTicksToPeriod() + "\n" + "CurrentRadius".Translate().CapitalizeFirst() + ": " + CurrentRadius.ToString("F1");
			if (ProgressDays < Props.daysToRadius && Working)
			{
				string text2 = text;
				text = text2 + "\n" + "RadiusExpandsIn".Translate().CapitalizeFirst() + ": " + TicksUntilRadiusInteger.ToStringTicksToPeriod();
			}
			return text;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "DEBUG: Progress 1 day",
					action = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_004c: stateMachine*/)._0024this.progressTicks += 60000;
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
