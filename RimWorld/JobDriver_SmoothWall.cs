using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SmoothWall : JobDriver
	{
		private float workLeft = -1000f;

		protected int BaseWorkAmount => 6500;

		protected DesignationDef DesDef => DesignationDefOf.SmoothWall;

		protected StatDef SpeedStat => StatDefOf.SmoothingSpeed;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo targetA = base.job.targetA;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				targetA = base.job.targetA.Cell;
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				result = (pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0036: stateMachine*/;
			this.FailOn(delegate
			{
				if (!_003CMakeNewToils_003Ec__Iterator._0024this.job.ignoreDesignations && _003CMakeNewToils_003Ec__Iterator._0024this.Map.designationManager.DesignationAt(_003CMakeNewToils_003Ec__Iterator._0024this.TargetLocA, _003CMakeNewToils_003Ec__Iterator._0024this.DesDef) == null)
				{
					return true;
				}
				return false;
			});
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		protected void DoEffect()
		{
			SmoothableWallUtility.Notify_SmoothedByPawn(SmoothableWallUtility.SmoothWall(base.TargetA.Thing, pawn), pawn);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		}
	}
}
