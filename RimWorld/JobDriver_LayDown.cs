using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayDown : JobDriver
	{
		public const TargetIndex BedOrRestSpotIndex = TargetIndex.A;

		public Building_Bed Bed => (Building_Bed)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (base.job.GetTarget(TargetIndex.A).HasThing)
			{
				Pawn pawn = base.pawn;
				LocalTargetInfo target = Bed;
				Job job = base.job;
				int sleepingSlotsCount = Bed.SleepingSlotsCount;
				int stackCount = 0;
				bool errorOnFailed2 = errorOnFailed;
				if (!pawn.Reserve(target, job, sleepingSlotsCount, stackCount, null, errorOnFailed2))
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!job.GetTarget(TargetIndex.A).HasThing)
			{
				yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override string GetReport()
		{
			if (asleep)
			{
				return "ReportSleeping".Translate();
			}
			return "ReportResting".Translate();
		}
	}
}
