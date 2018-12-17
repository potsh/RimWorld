using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Strip : JobDriver
	{
		private const int StripTicks = 60;

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
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !StrippableUtility.CanBeStrippedByColony(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.TargetThingA));
			Toil gotoThing = new Toil
			{
				initAction = delegate
				{
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006c: stateMachine*/)._0024this.pawn.pather.StartPath(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006c: stateMachine*/)._0024this.TargetThingA, PathEndMode.ClosestTouch);
				},
				defaultCompleteMode = ToilCompleteMode.PatherArrival
			};
			gotoThing.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return gotoThing;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override object[] TaleParameters()
		{
			Corpse corpse = base.TargetA.Thing as Corpse;
			return new object[2]
			{
				pawn,
				(corpse == null) ? base.TargetA.Thing : corpse.InnerPawn
			};
		}
	}
}
