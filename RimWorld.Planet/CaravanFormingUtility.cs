using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanFormingUtility
	{
		private static readonly Texture2D RemoveFromCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/RemoveFromCaravan");

		private static readonly Texture2D AddToCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/AddToCaravan");

		private static List<ThingCount> tmpCaravanPawns = new List<ThingCount>();

		public static void FormAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile, int destinationTile)
		{
			CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile, destinationTile);
		}

		public static void StartFormingCaravan(List<Pawn> pawns, List<Pawn> downedPawns, Faction faction, List<TransferableOneWay> transferables, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile, int destinationTile)
		{
			if (startingTile < 0)
			{
				Log.Error("Can't start forming caravan because startingTile is invalid.");
			}
			else if (!pawns.Any())
			{
				Log.Error("Can't start forming caravan with 0 pawns.");
			}
			else
			{
				if (pawns.Any((Pawn x) => x.Downed))
				{
					Log.Warning("Forming a caravan with a downed pawn. This shouldn't happen because we have to create a Lord.");
				}
				List<TransferableOneWay> list = transferables.ToList();
				list.RemoveAll((TransferableOneWay x) => x.CountToTransfer <= 0 || !x.HasAnyThing || x.AnyThing is Pawn);
				for (int i = 0; i < pawns.Count; i++)
				{
					pawns[i].GetLord()?.Notify_PawnLost(pawns[i], PawnLostCondition.ForcedToJoinOtherLord);
				}
				LordJob_FormAndSendCaravan lordJob = new LordJob_FormAndSendCaravan(list, downedPawns, meetingPoint, exitSpot, startingTile, destinationTile);
				LordMaker.MakeNewLord(Faction.OfPlayer, lordJob, pawns[0].MapHeld, pawns);
				for (int j = 0; j < pawns.Count; j++)
				{
					Pawn pawn = pawns[j];
					if (pawn.Spawned)
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}

		public static void StopFormingCaravan(Lord lord)
		{
			SetToUnloadEverything(lord);
			lord.lordManager.RemoveLord(lord);
		}

		public static void RemovePawnFromCaravan(Pawn pawn, Lord lord, bool removeFromDowned = true)
		{
			bool flag = false;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn2 = lord.ownedPawns[i];
				if (pawn2 != pawn && CaravanUtility.IsOwner(pawn2, Faction.OfPlayer))
				{
					flag = true;
					break;
				}
			}
			bool flag2 = true;
			string text = "MessagePawnLostWhileFormingCaravan".Translate(pawn).CapitalizeFirst();
			if (!flag)
			{
				StopFormingCaravan(lord);
				text = text + " " + "MessagePawnLostWhileFormingCaravan_AllLost".Translate();
			}
			else
			{
				pawn.inventory.UnloadEverything = true;
				if (lord.ownedPawns.Contains(pawn))
				{
					lord.Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction);
					flag2 = false;
				}
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lord.LordJob as LordJob_FormAndSendCaravan;
				if (lordJob_FormAndSendCaravan != null && lordJob_FormAndSendCaravan.downedPawns.Contains(pawn))
				{
					if (!removeFromDowned)
					{
						flag2 = false;
					}
					else
					{
						lordJob_FormAndSendCaravan.downedPawns.Remove(pawn);
					}
				}
			}
			if (flag2)
			{
				Messages.Message(text, pawn, MessageTypeDefOf.NegativeEvent);
			}
		}

		public static void Notify_FormAndSendCaravanLordFailed(Lord lord)
		{
			SetToUnloadEverything(lord);
		}

		private static void SetToUnloadEverything(Lord lord)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].inventory.UnloadEverything = true;
			}
		}

		public static List<Thing> AllReachableColonyItems(Map map, bool allowEvenIfOutsideHomeArea = false, bool allowEvenIfReserved = false, bool canMinify = false)
		{
			List<Thing> list = new List<Thing>();
			List<Thing> allThings = map.listerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				bool flag = canMinify && thing.def.Minifiable;
				if ((flag || thing.def.category == ThingCategory.Item) && (allowEvenIfOutsideHomeArea || map.areaManager.Home[thing.Position] || thing.IsInAnyStorage()) && !thing.Position.Fogged(thing.Map) && (allowEvenIfReserved || !map.reservationManager.IsReservedByAnyoneOf(thing, Faction.OfPlayer)) && (flag || thing.def.EverHaulable))
				{
					list.Add(thing);
				}
			}
			return list;
		}

		public static List<Pawn> AllSendablePawns(Map map, bool allowEvenIfDowned = false, bool allowEvenIfInMentalState = false, bool allowEvenIfPrisonerNotSecure = false, bool allowCapturableDownedPawns = false)
		{
			List<Pawn> list = new List<Pawn>();
			List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn = allPawnsSpawned[i];
				if ((allowEvenIfDowned || !pawn.Downed) && (allowEvenIfInMentalState || !pawn.InMentalState) && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony || (allowCapturableDownedPawns && CanListAsAutoCapturable(pawn))) && (allowEvenIfPrisonerNotSecure || !pawn.IsPrisoner || pawn.guest.PrisonerIsSecure) && (pawn.GetLord() == null || pawn.GetLord().LordJob is LordJob_VoluntarilyJoinable))
				{
					list.Add(pawn);
				}
			}
			return list;
		}

		private static bool CanListAsAutoCapturable(Pawn p)
		{
			return p.Downed && !p.mindState.WillJoinColonyIfRescued && CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer);
		}

		public static IEnumerable<Gizmo> GetGizmos(Pawn pawn)
		{
			if (IsFormingCaravanOrDownedPawnToBeTakenByCaravan(pawn))
			{
				_003CGetGizmos_003Ec__Iterator0 _003CGetGizmos_003Ec__Iterator = (_003CGetGizmos_003Ec__Iterator0)/*Error near IL_006b: stateMachine*/;
				Lord lord = GetFormAndSendCaravanLord(pawn);
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandCancelFormingCaravan".Translate(),
					defaultDesc = "CommandCancelFormingCaravanDesc".Translate(),
					icon = TexCommand.ClearPrioritizedWork,
					activateSound = SoundDefOf.Tick_Low,
					action = delegate
					{
						StopFormingCaravan(lord);
					},
					hotKey = KeyBindingDefOf.Designator_Cancel
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (pawn.Spawned)
			{
				bool anyCaravanToJoin = false;
				for (int i = 0; i < pawn.Map.lordManager.lords.Count; i++)
				{
					Lord lord2 = pawn.Map.lordManager.lords[i];
					if (lord2.faction == Faction.OfPlayer && lord2.LordJob is LordJob_FormAndSendCaravan)
					{
						anyCaravanToJoin = true;
						break;
					}
				}
				if (anyCaravanToJoin && Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
				{
					yield return (Gizmo)new Command_Action
					{
						defaultLabel = "CommandAddToCaravan".Translate(),
						defaultDesc = "CommandAddToCaravanDesc".Translate(),
						icon = AddToCaravanCommand,
						action = delegate
						{
							List<Lord> list = new List<Lord>();
							for (int j = 0; j < pawn.Map.lordManager.lords.Count; j++)
							{
								Lord lord3 = pawn.Map.lordManager.lords[j];
								if (lord3.faction == Faction.OfPlayer && lord3.LordJob is LordJob_FormAndSendCaravan)
								{
									list.Add(lord3);
								}
							}
							if (list.Count != 0)
							{
								if (list.Count == 1)
								{
									LateJoinFormingCaravan(pawn, list[0]);
									SoundDefOf.Click.PlayOneShotOnCamera();
								}
								else
								{
									List<FloatMenuOption> list2 = new List<FloatMenuOption>();
									for (int k = 0; k < list.Count; k++)
									{
										Lord caravanLocal = list[k];
										string label = "Caravan".Translate() + " " + (k + 1);
										list2.Add(new FloatMenuOption(label, delegate
										{
											if (pawn.Spawned && pawn.Map.lordManager.lords.Contains(caravanLocal) && Dialog_FormCaravan.AllSendablePawns(pawn.Map, reform: false).Contains(pawn))
											{
												LateJoinFormingCaravan(pawn, caravanLocal);
											}
										}));
									}
									Find.WindowStack.Add(new FloatMenu(list2));
								}
							}
						},
						hotKey = KeyBindingDefOf.Misc7
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		private static void LateJoinFormingCaravan(Pawn pawn, Lord lord)
		{
			pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord);
			if (pawn.Downed)
			{
				((LordJob_FormAndSendCaravan)lord.LordJob).downedPawns.Add(pawn);
			}
			else
			{
				lord.AddPawn(pawn);
			}
			if (pawn.Spawned)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public static bool IsFormingCaravan(this Pawn p)
		{
			Lord lord = p.GetLord();
			return lord != null && lord.LordJob is LordJob_FormAndSendCaravan;
		}

		public static bool IsFormingCaravanOrDownedPawnToBeTakenByCaravan(Pawn p)
		{
			return GetFormAndSendCaravanLord(p) != null;
		}

		public static Lord GetFormAndSendCaravanLord(Pawn p)
		{
			if (p.IsFormingCaravan())
			{
				return p.GetLord();
			}
			if (p.Spawned)
			{
				List<Lord> lords = p.Map.lordManager.lords;
				for (int i = 0; i < lords.Count; i++)
				{
					LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = lords[i].LordJob as LordJob_FormAndSendCaravan;
					if (lordJob_FormAndSendCaravan != null && lordJob_FormAndSendCaravan.downedPawns.Contains(p))
					{
						return lords[i];
					}
				}
			}
			return null;
		}

		public static float CapacityLeft(LordJob_FormAndSendCaravan lordJob)
		{
			float num = CollectionsMassCalculator.MassUsageTransferables(lordJob.transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
			tmpCaravanPawns.Clear();
			for (int i = 0; i < lordJob.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lordJob.lord.ownedPawns[i];
				tmpCaravanPawns.Add(new ThingCount(pawn, pawn.stackCount));
			}
			num += CollectionsMassCalculator.MassUsage(tmpCaravanPawns, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
			float num2 = CollectionsMassCalculator.Capacity(tmpCaravanPawns);
			tmpCaravanPawns.Clear();
			return num2 - num;
		}

		public static string AppendOverweightInfo(string text, float capacityLeft)
		{
			if (capacityLeft < 0f)
			{
				text = text + " (" + "OverweightLower".Translate() + ")";
			}
			return text;
		}
	}
}
