using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Resurrect : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex ItemInd = TargetIndex.B;

		private const int DurationTicks = 600;

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private Thing Item => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Corpse;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = Item;
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				result = (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private void Resurrect()
		{
			Pawn innerPawn = Corpse.InnerPawn;
			ResurrectionUtility.ResurrectWithSideEffects(innerPawn);
			Messages.Message("MessagePawnResurrected".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent);
			Item.SplitOff(1).Destroy();
		}
	}
}
