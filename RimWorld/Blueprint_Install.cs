using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Blueprint_Install : Blueprint
	{
		private MinifiedThing miniToInstall;

		private Building buildingToReinstall;

		public Thing MiniToInstallOrBuildingToReinstall
		{
			get
			{
				if (miniToInstall != null)
				{
					return miniToInstall;
				}
				if (buildingToReinstall != null)
				{
					return buildingToReinstall;
				}
				throw new InvalidOperationException("Nothing to install.");
			}
		}

		private Thing ThingToInstall => MiniToInstallOrBuildingToReinstall.GetInnerIfMinified();

		public override Graphic Graphic
		{
			get
			{
				Graphic graphic = ThingToInstall.def.installBlueprintDef.graphic;
				return graphic.ExtractInnerGraphicFor(ThingToInstall);
			}
		}

		protected override float WorkTotal => 150f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref miniToInstall, "miniToInstall");
			Scribe_References.Look(ref buildingToReinstall, "buildingToReinstall");
		}

		public override ThingDef UIStuff()
		{
			return ThingToInstall.Stuff;
		}

		public override List<ThingDefCountClass> MaterialsNeeded()
		{
			Log.Error("Called MaterialsNeeded on a Blueprint_Install.");
			return new List<ThingDefCountClass>();
		}

		protected override Thing MakeSolidThing()
		{
			Thing thingToInstall = ThingToInstall;
			if (miniToInstall != null)
			{
				miniToInstall.InnerThing = null;
				miniToInstall.Destroy();
			}
			return thingToInstall;
		}

		public override bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			Map map = base.Map;
			bool flag = base.TryReplaceWithSolidThing(workerPawn, out createdThing, out jobEnded);
			if (flag)
			{
				SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(base.Position, map));
				workerPawn.records.Increment(RecordDefOf.ThingsInstalled);
			}
			return flag;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(ThingToInstall.def, ThingToInstall.Stuff);
			if (buildCopy != null)
			{
				yield return (Gizmo)buildCopy;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				using (IEnumerator<Command> enumerator2 = BuildFacilityCommandUtility.BuildFacilityCommands(ThingToInstall.def).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Command facility = enumerator2.Current;
						yield return (Gizmo)facility;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01c3:
			/*Error near IL_01c4: Unexpected return in MoveNext()*/;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (buildingToReinstall != null)
			{
				GenDraw.DrawLineBetween(buildingToReinstall.TrueCenter(), this.TrueCenter());
			}
		}

		internal void SetThingToInstallFromMinified(MinifiedThing itemToInstall)
		{
			miniToInstall = itemToInstall;
			buildingToReinstall = null;
		}

		internal void SetBuildingToReinstall(Building buildingToReinstall)
		{
			if (!buildingToReinstall.def.Minifiable)
			{
				Log.Error("Tried to reinstall non-minifiable building.");
			}
			else
			{
				miniToInstall = null;
				this.buildingToReinstall = buildingToReinstall;
			}
		}
	}
}
