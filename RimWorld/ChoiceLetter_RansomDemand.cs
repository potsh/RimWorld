using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_RansomDemand : ChoiceLetter
	{
		public Map map;

		public Faction faction;

		public Pawn kidnapped;

		public int fee;

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (!base.ArchivedOnly)
				{
					DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
					{
						action = delegate
						{
							((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.faction.kidnapped.RemoveKidnappedPawn(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.kidnapped);
							Find.WorldPawns.RemovePawn(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.kidnapped);
							IntVec3 result;
							if ((int)((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.faction.def.techLevel < 4)
							{
								if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map) && ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map.reachability.CanReachColony(c), ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map, CellFinder.EdgeRoadChance_Friendly, out result) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map), ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map, CellFinder.EdgeRoadChance_Friendly, out result))
								{
									Log.Warning("Could not find any edge cell.");
									result = DropCellFinder.TradeDropSpot(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map);
								}
								GenSpawn.Spawn(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.kidnapped, result, ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map);
							}
							else
							{
								result = DropCellFinder.TradeDropSpot(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map);
								TradeUtility.SpawnDropPod(result, ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map, ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.kidnapped);
							}
							CameraJumper.TryJump(result, ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map);
							TradeUtility.LaunchSilver(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.map, ((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this.fee);
							Find.LetterStack.RemoveLetter(((_003C_003Ec__Iterator0)/*Error near IL_0082: stateMachine*/)._0024this);
						},
						resolveTree = true
					};
					if (!TradeUtility.ColonyHasEnoughSilver(map, fee))
					{
						accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
					}
					yield return accept;
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return base.Option_Close;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override bool CanShowInLetterStack
		{
			get
			{
				if (!base.CanShowInLetterStack)
				{
					return false;
				}
				if (!Find.Maps.Contains(map))
				{
					return false;
				}
				return faction.kidnapped.KidnappedPawnsListForReading.Contains(kidnapped);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref map, "map");
			Scribe_References.Look(ref faction, "faction");
			Scribe_References.Look(ref kidnapped, "kidnapped");
			Scribe_Values.Look(ref fee, "fee", 0);
		}
	}
}
