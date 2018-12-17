using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SocialRelax : JobDriver
	{
		private const TargetIndex GatherSpotParentInd = TargetIndex.A;

		private const TargetIndex ChairOrSpotInd = TargetIndex.B;

		private const TargetIndex OptionalIngestibleInd = TargetIndex.C;

		private Thing GatherSpotParent => job.GetTarget(TargetIndex.A).Thing;

		private bool HasChair => job.GetTarget(TargetIndex.B).HasThing;

		private bool HasDrink => job.GetTarget(TargetIndex.C).HasThing;

		private IntVec3 ClosestGatherSpotParentCell => GatherSpotParent.OccupiedRect().ClosestCellTo(pawn.Position);

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = base.job.GetTarget(TargetIndex.B);
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			if (HasDrink)
			{
				pawn = base.pawn;
				target = base.job.GetTarget(TargetIndex.C);
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
			this.EndOnDespawnedOrNull(TargetIndex.A);
			if (HasChair)
			{
				this.EndOnDespawnedOrNull(TargetIndex.B);
			}
			if (!HasDrink)
			{
				yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
			yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.C);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 closestGatherSpotParentCell = ClosestGatherSpotParentCell;
			return JobDriver_Ingest.ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, closestGatherSpotParentCell, pawn);
		}
	}
}
