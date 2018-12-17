using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class MarriageCeremonyUtility
	{
		public static bool AcceptableGameConditionsToStartCeremony(Map map)
		{
			if (!AcceptableGameConditionsToContinueCeremony(map))
			{
				return false;
			}
			if (GenLocalDate.HourInteger(map) < 5 || GenLocalDate.HourInteger(map) > 16)
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
			int num = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.Drafted)
				{
					num++;
				}
			}
			if ((float)num / (float)map.mapPawns.FreeColonistsSpawnedCount >= 0.5f)
			{
				return false;
			}
			return true;
		}

		public static bool AcceptableGameConditionsToContinueCeremony(Map map)
		{
			if (map.dangerWatcher.DangerRating == StoryDanger.High)
			{
				return false;
			}
			return true;
		}

		public static bool FianceReadyToStartCeremony(Pawn pawn, Pawn otherPawn)
		{
			if (!FianceCanContinueCeremony(pawn, otherPawn))
			{
				return false;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0f)
			{
				return false;
			}
			if (HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
			{
				return false;
			}
			if (PawnUtility.WillSoonHaveBasicNeed(pawn))
			{
				return false;
			}
			if (IsCurrentlyMarryingSomeone(pawn))
			{
				return false;
			}
			if (pawn.GetLord() != null)
			{
				return false;
			}
			return !pawn.Drafted && !pawn.InMentalState && pawn.Awake() && !pawn.IsBurning() && !pawn.InBed();
		}

		public static bool FianceCanContinueCeremony(Pawn pawn, Pawn otherPawn)
		{
			if (pawn.health.hediffSet.BleedRateTotal > 0.3f)
			{
				return false;
			}
			if (pawn.IsPrisoner)
			{
				return false;
			}
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.2f)
			{
				return false;
			}
			if (pawn.HostileTo(otherPawn))
			{
				return false;
			}
			if (pawn.IsWildMan())
			{
				return false;
			}
			return pawn.Spawned && !pawn.Downed && !pawn.InMentalState;
		}

		public static bool ShouldGuestKeepAttendingCeremony(Pawn p)
		{
			return GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
		}

		public static void Married(Pawn firstPawn, Pawn secondPawn)
		{
			LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse(firstPawn);
			LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse(secondPawn);
			firstPawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, secondPawn);
			firstPawn.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExSpouse, secondPawn);
			firstPawn.relations.AddDirectRelation(PawnRelationDefOf.Spouse, secondPawn);
			AddNewlyMarriedThoughts(firstPawn, secondPawn);
			AddNewlyMarriedThoughts(secondPawn, firstPawn);
			firstPawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.DivorcedMe, secondPawn);
			secondPawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.DivorcedMe, firstPawn);
			LovePartnerRelationUtility.TryToShareBed(firstPawn, secondPawn);
			TaleRecorder.RecordTale(TaleDefOf.Marriage, firstPawn, secondPawn);
		}

		private static void AddNewlyMarriedThoughts(Pawn pawn, Pawn otherPawn)
		{
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GotMarried, otherPawn);
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HoneymoonPhase, otherPawn);
		}

		private static bool IsCurrentlyMarryingSomeone(Pawn p)
		{
			if (!p.Spawned)
			{
				return false;
			}
			List<Lord> lords = p.Map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				LordJob_Joinable_MarriageCeremony lordJob_Joinable_MarriageCeremony = lords[i].LordJob as LordJob_Joinable_MarriageCeremony;
				if (lordJob_Joinable_MarriageCeremony != null && (lordJob_Joinable_MarriageCeremony.firstPawn == p || lordJob_Joinable_MarriageCeremony.secondPawn == p))
				{
					return true;
				}
			}
			return false;
		}
	}
}
