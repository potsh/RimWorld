using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RelaxAlone : JobDriver
	{
		private Rot4 faceDir = Rot4.Invalid;

		private const TargetIndex SpotOrBedInd = TargetIndex.A;

		private bool FromBed => job.GetTarget(TargetIndex.A).HasThing;

		public override bool CanBeginNowWhileLyingDown()
		{
			return FromBed && JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (FromBed)
			{
				Pawn pawn = base.pawn;
				LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
				Job job = base.job;
				int sleepingSlotsCount = ((Building_Bed)base.job.GetTarget(TargetIndex.A).Thing).SleepingSlotsCount;
				int stackCount = 0;
				bool errorOnFailed2 = errorOnFailed;
				if (!pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed2))
				{
					return false;
				}
			}
			else
			{
				Pawn pawn = base.pawn;
				LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
				Job job = base.job;
				bool errorOnFailed2 = errorOnFailed;
				if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
				{
					return false;
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!FromBed)
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			this.KeepLyingDown(TargetIndex.A);
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref faceDir, "faceDir");
		}
	}
}
