using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TendPatient : JobDriver
	{
		private bool usesMedicine;

		private const int BaseTendDuration = 600;

		protected Thing MedicineUsed => job.targetB.Thing;

		protected Pawn Deliveree => (Pawn)job.targetA.Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref usesMedicine, "usesMedicine", defaultValue: false);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			usesMedicine = (MedicineUsed != null);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Deliveree;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			if (usesMedicine)
			{
				int num = base.pawn.Map.reservationManager.CanReserveStack(base.pawn, MedicineUsed, 10);
				if (num > 0)
				{
					pawn = base.pawn;
					target = MedicineUsed;
					job = base.job;
					int maxPawns = 10;
					int stackCount = Mathf.Min(num, Medicine.GetMedicineCountToFullyHeal(Deliveree));
					errorOnFailed2 = errorOnFailed;
					if (pawn.Reserve(target, job, maxPawns, stackCount, null, errorOnFailed2))
					{
						goto IL_00b7;
					}
				}
				return false;
			}
			goto IL_00b7;
			IL_00b7:
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0052: stateMachine*/;
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(delegate
			{
				if (!WorkGiver_Tend.GoodLayingStatusForTend(_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree, _003CMakeNewToils_003Ec__Iterator._0024this.pawn))
				{
					return true;
				}
				if (_003CMakeNewToils_003Ec__Iterator._0024this.MedicineUsed != null && _003CMakeNewToils_003Ec__Iterator._0024this.pawn.Faction == Faction.OfPlayer)
				{
					if (_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree.playerSettings == null)
					{
						return true;
					}
					if (!_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree.playerSettings.medCare.AllowsMedicine(_003CMakeNewToils_003Ec__Iterator._0024this.MedicineUsed.def))
					{
						return true;
					}
				}
				if (_003CMakeNewToils_003Ec__Iterator._0024this.pawn == _003CMakeNewToils_003Ec__Iterator._0024this.Deliveree && _003CMakeNewToils_003Ec__Iterator._0024this.pawn.Faction == Faction.OfPlayer && !_003CMakeNewToils_003Ec__Iterator._0024this.pawn.playerSettings.selfTend)
				{
					return true;
				}
				return false;
			});
			AddEndCondition(delegate
			{
				if (_003CMakeNewToils_003Ec__Iterator._0024this.pawn.Faction == Faction.OfPlayer && HealthAIUtility.ShouldBeTendedNowByPlayer(_003CMakeNewToils_003Ec__Iterator._0024this.Deliveree))
				{
					return JobCondition.Ongoing;
				}
				if (_003CMakeNewToils_003Ec__Iterator._0024this.pawn.Faction != Faction.OfPlayer && _003CMakeNewToils_003Ec__Iterator._0024this.Deliveree.health.HasHediffsNeedingTend())
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Succeeded;
			});
			this.FailOnAggroMentalState(TargetIndex.A);
			Toil reserveMedicine = null;
			if (!usesMedicine)
			{
				PathEndMode interactionCell = (Deliveree == pawn) ? PathEndMode.OnCell : PathEndMode.InteractionCell;
				Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, interactionCell);
				yield return gotoToil;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			reserveMedicine = Toils_Tend.ReserveMedicine(TargetIndex.B, Deliveree).FailOnDespawnedNullOrForbidden(TargetIndex.B);
			yield return reserveMedicine;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void Notify_DamageTaken(DamageInfo dinfo)
		{
			base.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(pawn) && pawn.Faction != Faction.OfPlayer && pawn == Deliveree)
			{
				pawn.jobs.CheckForJobOverride();
			}
		}
	}
}
