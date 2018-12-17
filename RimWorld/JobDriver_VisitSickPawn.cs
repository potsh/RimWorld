using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitSickPawn : JobDriver
	{
		private const TargetIndex PatientInd = TargetIndex.A;

		private const TargetIndex ChairInd = TargetIndex.B;

		private Pawn Patient => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private Thing Chair => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Patient;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			if (Chair != null)
			{
				pawn = base.pawn;
				target = Chair;
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
				{
					return false;
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0040: stateMachine*/)._0024this.Patient.InBed() || !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0040: stateMachine*/)._0024this.Patient.Awake());
			if (Chair != null)
			{
				this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			}
			if (Chair == null)
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
