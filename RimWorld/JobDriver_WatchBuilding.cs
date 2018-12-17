using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_WatchBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo targetA = base.job.targetA;
			Job job = base.job;
			int joyMaxParticipants = base.job.def.joyMaxParticipants;
			int stackCount = 0;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(targetA, job, joyMaxParticipants, stackCount, null, errorOnFailed2))
			{
				return false;
			}
			pawn = base.pawn;
			targetA = base.job.targetB;
			job = base.job;
			errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			if (base.TargetC.HasThing)
			{
				if (base.TargetC.Thing is Building_Bed)
				{
					pawn = base.pawn;
					LocalTargetInfo targetC = base.job.targetC;
					job = base.job;
					stackCount = ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount;
					joyMaxParticipants = 0;
					errorOnFailed2 = errorOnFailed;
					if (!pawn.Reserve(targetC, job, stackCount, joyMaxParticipants, null, errorOnFailed2))
					{
						return false;
					}
				}
				else
				{
					pawn = base.pawn;
					LocalTargetInfo targetC = base.job.targetC;
					job = base.job;
					errorOnFailed2 = errorOnFailed;
					if (!pawn.Reserve(targetC, job, 1, -1, null, errorOnFailed2))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override bool CanBeginNowWhileLyingDown()
		{
			return base.TargetC.HasThing && base.TargetC.Thing is Building_Bed && JobInBedUtility.InBedOrRestSpotNow(pawn, base.TargetC);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_003e: stateMachine*/;
			this.EndOnDespawnedOrNull(TargetIndex.A);
			if (!base.TargetC.HasThing || !(base.TargetC.Thing is Building_Bed))
			{
				yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			this.KeepLyingDown(TargetIndex.C);
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		protected virtual void WatchTickAction()
		{
			base.pawn.rotationTracker.FaceCell(base.TargetA.Cell);
			base.pawn.GainComfortFromCellIfPossible();
			Pawn pawn = base.pawn;
			Building joySource = (Building)base.TargetThingA;
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, joySource);
		}

		public override object[] TaleParameters()
		{
			return new object[2]
			{
				pawn,
				base.TargetA.Thing.def
			};
		}
	}
}
