using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompSpawner : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_Spawner PropsSpawner => (CompProperties_Spawner)props;

		private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				ResetCountdown();
			}
		}

		public override void CompTick()
		{
			TickInterval(1);
		}

		public override void CompTickRare()
		{
			TickInterval(250);
		}

		private void TickInterval(int interval)
		{
			if (parent.Spawned)
			{
				Hive hive = parent as Hive;
				if (hive != null)
				{
					if (!hive.active)
					{
						return;
					}
				}
				else if (parent.Position.Fogged(parent.Map))
				{
					return;
				}
				if (!PropsSpawner.requiresPower || PowerOn)
				{
					ticksUntilSpawn -= interval;
					CheckShouldSpawn();
				}
			}
		}

		private void CheckShouldSpawn()
		{
			if (ticksUntilSpawn <= 0)
			{
				TryDoSpawn();
				ResetCountdown();
			}
		}

		public bool TryDoSpawn()
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (PropsSpawner.spawnMaxAdjacent >= 0)
			{
				int num = 0;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
					if (c.InBounds(parent.Map))
					{
						List<Thing> thingList = c.GetThingList(parent.Map);
						for (int j = 0; j < thingList.Count; j++)
						{
							if (thingList[j].def == PropsSpawner.thingToSpawn)
							{
								num += thingList[j].stackCount;
								if (num >= PropsSpawner.spawnMaxAdjacent)
								{
									return false;
								}
							}
						}
					}
				}
			}
			if (TryFindSpawnCell(out IntVec3 result))
			{
				Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
				thing.stackCount = PropsSpawner.spawnCount;
				if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
				{
					thing.SetFaction(parent.Faction);
				}
				GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out Thing lastResultingThing);
				if (PropsSpawner.spawnForbidden)
				{
					lastResultingThing.SetForbidden(value: true);
				}
				if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
				{
					Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap).CapitalizeFirst(), thing, MessageTypeDefOf.PositiveEvent);
				}
				return true;
			}
			return false;
		}

		private bool TryFindSpawnCell(out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
			{
				if (item.Walkable(parent.Map))
				{
					Building edifice = item.GetEdifice(parent.Map);
					if (edifice == null || !PropsSpawner.thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, item, parent.Map)))
						{
							bool flag = false;
							List<Thing> thingList = item.GetThingList(parent.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								Thing thing = thingList[i];
								if (thing.def.category == ThingCategory.Item && (thing.def != PropsSpawner.thingToSpawn || thing.stackCount > PropsSpawner.thingToSpawn.stackLimit - PropsSpawner.spawnCount))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								result = item;
								return true;
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		private void ResetCountdown()
		{
			ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		public override void PostExposeData()
		{
			string str = (!PropsSpawner.saveKeysPrefix.NullOrEmpty()) ? (PropsSpawner.saveKeysPrefix + "_") : null;
			Scribe_Values.Look(ref ticksUntilSpawn, str + "ticksUntilSpawn", 0);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "DEBUG: Spawn " + PropsSpawner.thingToSpawn.label,
					icon = TexCommand.DesirePower,
					action = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0076: stateMachine*/)._0024this.TryDoSpawn();
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0076: stateMachine*/)._0024this.ResetCountdown();
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
			{
				return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)) + ": " + ticksUntilSpawn.ToStringTicksToPeriod();
			}
			return null;
		}
	}
}
