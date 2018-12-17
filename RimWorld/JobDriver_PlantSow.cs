using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlantSow : JobDriver
	{
		private float sowWorkDone;

		private Plant Plant => (Plant)job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref sowWorkDone, "sowWorkDone", 0f);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo targetA = base.job.targetA;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0036: stateMachine*/;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => PlantUtility.AdjacentSowBlocker(_003CMakeNewToils_003Ec__Iterator._0024this.job.plantDefToSow, _003CMakeNewToils_003Ec__Iterator._0024this.TargetA.Cell, _003CMakeNewToils_003Ec__Iterator._0024this.Map) != null).FailOn(() => !_003CMakeNewToils_003Ec__Iterator._0024this.job.plantDefToSow.CanEverPlantAt(_003CMakeNewToils_003Ec__Iterator._0024this.TargetLocA, _003CMakeNewToils_003Ec__Iterator._0024this.Map));
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
