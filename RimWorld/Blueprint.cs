using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Blueprint : ThingWithComps, IConstructible
	{
		private static List<CompSpawnerMechanoidsOnDamaged> tmpCrashedShipParts = new List<CompSpawnerMechanoidsOnDamaged>();

		public override string Label => def.entityDefToBuild.label + "BlueprintLabelExtra".Translate();

		protected abstract float WorkTotal
		{
			get;
		}

		public override void Draw()
		{
			if (def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			else
			{
				Comps_PostDraw();
			}
		}

		public virtual bool TryReplaceWithSolidThing(Pawn workerPawn, out Thing createdThing, out bool jobEnded)
		{
			jobEnded = false;
			if (GenConstruct.FirstBlockingThing(this, workerPawn) != null)
			{
				workerPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				jobEnded = true;
				createdThing = null;
				return false;
			}
			createdThing = MakeSolidThing();
			Map map = base.Map;
			CellRect cellRect = this.OccupiedRect();
			GenSpawn.WipeExistingThings(base.Position, base.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
			if (!base.Destroyed)
			{
				Destroy();
			}
			createdThing.SetFactionDirect(workerPawn.Faction);
			GenSpawn.Spawn(createdThing, base.Position, map, base.Rotation);
			tmpCrashedShipParts.Clear();
			CellRect.CellRectIterator iterator = cellRect.ExpandedBy(3).GetIterator();
			while (!iterator.Done())
			{
				if (iterator.Current.InBounds(map))
				{
					List<Thing> thingList = iterator.Current.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						CompSpawnerMechanoidsOnDamaged compSpawnerMechanoidsOnDamaged = thingList[i].TryGetComp<CompSpawnerMechanoidsOnDamaged>();
						if (compSpawnerMechanoidsOnDamaged != null)
						{
							tmpCrashedShipParts.Add(compSpawnerMechanoidsOnDamaged);
						}
					}
				}
				iterator.MoveNext();
			}
			tmpCrashedShipParts.RemoveDuplicates();
			for (int j = 0; j < tmpCrashedShipParts.Count; j++)
			{
				tmpCrashedShipParts[j].Notify_BlueprintReplacedWithSolidThingNearby(workerPawn);
			}
			tmpCrashedShipParts.Clear();
			return true;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			map.blueprintGrid.Register(this);
			base.SpawnSetup(map, respawningAfterLoad);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			map.blueprintGrid.DeRegister(this);
		}

		protected abstract Thing MakeSolidThing();

		public abstract List<ThingDefCountClass> MaterialsNeeded();

		public abstract ThingDef UIStuff();

		public Thing BlockingHaulableOnTop()
		{
			if (def.entityDefToBuild.passability == Traversability.Standable)
			{
				return null;
			}
			CellRect.CellRectIterator iterator = this.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.EverHaulable)
					{
						return thing;
					}
				}
				iterator.MoveNext();
			}
			return null;
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (base.Faction == null)
			{
				return 0;
			}
			if (def.entityDefToBuild is TerrainDef)
			{
				return 0;
			}
			if ((p.Faction == base.Faction || p.HostFaction == base.Faction) && (base.Map.reservationManager.IsReservedByAnyoneOf(this, p.Faction) || (p.HostFaction != null && base.Map.reservationManager.IsReservedByAnyoneOf(this, p.HostFaction))))
			{
				return Frame.AvoidUnderConstructionPathFindCost;
			}
			return 0;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append("WorkLeft".Translate() + ": " + WorkTotal.ToStringWorkAmount());
			return stringBuilder.ToString();
		}
	}
}
