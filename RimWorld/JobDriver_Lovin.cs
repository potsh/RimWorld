using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Lovin : JobDriver
	{
		private int ticksLeft;

		private TargetIndex PartnerInd = TargetIndex.A;

		private TargetIndex BedInd = TargetIndex.B;

		private const int TicksBetweenHeartMotes = 100;

		private static readonly SimpleCurve LovinIntervalHoursFromAgeCurve = new SimpleCurve
		{
			new CurvePoint(16f, 1.5f),
			new CurvePoint(22f, 1.5f),
			new CurvePoint(30f, 4f),
			new CurvePoint(50f, 12f),
			new CurvePoint(75f, 36f)
		};

		private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);

		private Building_Bed Bed => (Building_Bed)(Thing)job.GetTarget(BedInd);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Partner;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = Bed;
				job = base.job;
				int sleepingSlotsCount = Bed.SleepingSlotsCount;
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

		public override bool CanBeginNowWhileLyingDown()
		{
			return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(BedInd));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(BedInd);
			this.FailOnDespawnedOrNull(PartnerInd);
			this.FailOn(() => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0061: stateMachine*/)._0024this.Partner.health.capacities.CanBeAwake);
			this.KeepLyingDown(BedInd);
			yield return Toils_Bed.ClaimBedIfNonMedical(BedInd);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private int GenerateRandomMinTicksToNextLovin(Pawn pawn)
		{
			if (DebugSettings.alwaysDoLovin)
			{
				return 100;
			}
			float centerX = LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			centerX = Rand.Gaussian(centerX, 0.3f);
			if (centerX < 0.5f)
			{
				centerX = 0.5f;
			}
			return (int)(centerX * 2500f);
		}
	}
}
