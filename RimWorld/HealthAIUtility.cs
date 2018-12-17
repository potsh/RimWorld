using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealthAIUtility
	{
		public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
		{
			return pawn.Downed || pawn.health.HasHediffsNeedingTend() || ShouldHaveSurgeryDoneNow(pawn);
		}

		public static bool ShouldSeekMedicalRest(Pawn pawn)
		{
			return ShouldSeekMedicalRestUrgent(pawn) || pawn.health.hediffSet.HasTendedAndHealingInjury() || pawn.health.hediffSet.HasImmunizableNotImmuneHediff();
		}

		public static bool ShouldBeTendedNowByPlayerUrgent(Pawn pawn)
		{
			return ShouldBeTendedNowByPlayer(pawn) && HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 45000;
		}

		public static bool ShouldBeTendedNowByPlayer(Pawn pawn)
		{
			if (pawn.playerSettings == null)
			{
				return false;
			}
			if (!ShouldEverReceiveMedicalCareFromPlayer(pawn))
			{
				return false;
			}
			return pawn.health.HasHediffsNeedingTendByPlayer();
		}

		public static bool ShouldEverReceiveMedicalCareFromPlayer(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.medCare == MedicalCareCategory.NoCare)
			{
				return false;
			}
			if (pawn.guest != null && pawn.guest.interactionMode == PrisonerInteractionModeDefOf.Execution)
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(pawn, DesignationDefOf.Slaughter) != null)
			{
				return false;
			}
			return true;
		}

		public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
		{
			return pawn.health.surgeryBills.AnyShouldDoNow;
		}

		public static Thing FindBestMedicine(Pawn healer, Pawn patient)
		{
			if (patient.playerSettings == null || (int)patient.playerSettings.medCare <= 1)
			{
				return null;
			}
			if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
			{
				return null;
			}
			Predicate<Thing> predicate = delegate(Thing m)
			{
				if (m.IsForbidden(healer) || !patient.playerSettings.medCare.AllowsMedicine(m.def) || !healer.CanReserve(m, 10, 1))
				{
					return false;
				}
				return true;
			};
			Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
			IntVec3 position = patient.Position;
			Map map = patient.Map;
			List<Thing> searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
			PathEndMode peMode = PathEndMode.ClosestTouch;
			TraverseParms traverseParams = TraverseParms.For(healer);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, priorityGetter);
		}
	}
}
