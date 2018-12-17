using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TakeToBed : JobDriver
	{
		private const TargetIndex TakeeIndex = TargetIndex.A;

		private const TargetIndex BedIndex = TargetIndex.B;

		protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		protected Building_Bed DropBed => (Building_Bed)job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Takee;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = DropBed;
				job = base.job;
				int sleepingSlotsCount = DropBed.SleepingSlotsCount;
				int stackCount = 0;
				errorOnFailed2 = errorOnFailed;
				result = (pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed2) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
			this.FailOn(delegate
			{
				if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006a: stateMachine*/)._0024this.job.def.makeTargetPrisoner)
				{
					if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006a: stateMachine*/)._0024this.DropBed.ForPrisoners)
					{
						return true;
					}
				}
				else if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006a: stateMachine*/)._0024this.DropBed.ForPrisoners != ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006a: stateMachine*/)._0024this.Takee.IsPrisoner)
				{
					return true;
				}
				return false;
			});
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private void CheckMakeTakeePrisoner()
		{
			if (job.def.makeTargetPrisoner)
			{
				if (Takee.guest.Released)
				{
					Takee.guest.Released = false;
					Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
				}
				if (!Takee.IsPrisonerOfColony)
				{
					Takee.guest.CapturedBy(Faction.OfPlayer, pawn);
				}
			}
		}

		private void CheckMakeTakeeGuest()
		{
			if (!job.def.makeTargetPrisoner && Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer && Takee.guest != null && !Takee.IsWildMan())
			{
				Takee.guest.SetGuestStatus(Faction.OfPlayer);
			}
		}
	}
}
