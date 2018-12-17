using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PartyUtility
	{
		private const float PartyAreaRadiusIfNotWholeRoom = 10f;

		private const int MaxRoomCellsCountToUseWholeRoom = 324;

		public static bool AcceptableGameConditionsToStartParty(Map map)
		{
			if (!AcceptableGameConditionsToContinueParty(map))
			{
				return false;
			}
			if (GenLocalDate.HourInteger(map) < 4 || GenLocalDate.HourInteger(map) > 21)
			{
				return false;
			}
			if (GatheringsUtility.AnyLordJobPreventsNewGatherings(map))
			{
				return false;
			}
			if (map.dangerWatcher.DangerRating != 0)
			{
				return false;
			}
			int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
			if (freeColonistsSpawnedCount < 4)
			{
				return false;
			}
			int num = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.health.hediffSet.BleedRateTotal > 0f)
				{
					return false;
				}
				if (item.Drafted)
				{
					num++;
				}
			}
			if ((float)num / (float)freeColonistsSpawnedCount >= 0.5f)
			{
				return false;
			}
			if (!EnoughPotentialGuestsToStartParty(map))
			{
				return false;
			}
			return true;
		}

		public static bool AcceptableGameConditionsToContinueParty(Map map)
		{
			if (map.dangerWatcher.DangerRating == StoryDanger.High)
			{
				return false;
			}
			return true;
		}

		public static bool EnoughPotentialGuestsToStartParty(Map map, IntVec3? partySpot = default(IntVec3?))
		{
			int value = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawnedCount * 0.65f);
			value = Mathf.Clamp(value, 2, 10);
			int num = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (ShouldPawnKeepPartying(item) && (!partySpot.HasValue || !partySpot.Value.IsForbidden(item)) && (!partySpot.HasValue || item.CanReach(partySpot.Value, PathEndMode.Touch, Danger.Some)))
				{
					num++;
				}
			}
			return num >= value;
		}

		public static Pawn FindRandomPartyOrganizer(Faction faction, Map map)
		{
			Predicate<Pawn> validator = (Pawn x) => x.RaceProps.Humanlike && !x.InBed() && !x.InMentalState && x.GetLord() == null && ShouldPawnKeepPartying(x);
			if ((from x in map.mapPawns.SpawnedPawnsInFaction(faction)
			where validator(x)
			select x).TryRandomElement(out Pawn result))
			{
				return result;
			}
			return null;
		}

		public static bool ShouldPawnKeepPartying(Pawn p)
		{
			if (p.timetable != null && !p.timetable.CurrentAssignment.allowJoy)
			{
				return false;
			}
			if (!GatheringsUtility.ShouldGuestKeepAttendingGathering(p))
			{
				return false;
			}
			return true;
		}

		public static bool InPartyArea(IntVec3 cell, IntVec3 partySpot, Map map)
		{
			if (UseWholeRoomAsPartyArea(partySpot, map) && cell.GetRoom(map) == partySpot.GetRoom(map))
			{
				return true;
			}
			if (cell.InHorDistOf(partySpot, 10f))
			{
				Building edifice = cell.GetEdifice(map);
				TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.None);
				if (edifice != null)
				{
					return map.reachability.CanReach(partySpot, edifice, PathEndMode.ClosestTouch, traverseParams);
				}
				return map.reachability.CanReach(partySpot, cell, PathEndMode.ClosestTouch, traverseParams);
			}
			return false;
		}

		public static bool TryFindRandomCellInPartyArea(Pawn pawn, out IntVec3 result)
		{
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None);
			if (UseWholeRoomAsPartyArea(cell, pawn.Map))
			{
				Room room = cell.GetRoom(pawn.Map);
				return (from x in room.Cells
				where validator(x)
				select x).TryRandomElement(out result);
			}
			return CellFinder.TryFindRandomReachableCellNear(cell, pawn.Map, 10f, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 x) => validator(x), null, out result, 10);
		}

		public static bool UseWholeRoomAsPartyArea(IntVec3 partySpot, Map map)
		{
			Room room = partySpot.GetRoom(map);
			if (room != null && !room.IsHuge && !room.PsychologicallyOutdoors && room.CellCount <= 324)
			{
				return true;
			}
			return false;
		}
	}
}
