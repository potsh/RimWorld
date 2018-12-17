using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_AttackMelee : JobDriver
	{
		private int numMeleeAttacksMade;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			IAttackTarget attackTarget = job.targetA.Thing as IAttackTarget;
			if (attackTarget != null)
			{
				pawn.Map.attackTargetReservationManager.Reserve(pawn, job, attackTarget);
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_General.DoAtomic(delegate
			{
				Pawn pawn = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002a: stateMachine*/)._0024this.job.targetA.Thing as Pawn;
				if (pawn != null && pawn.Downed && ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002a: stateMachine*/)._0024this.pawn.mindState.duty != null && ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002a: stateMachine*/)._0024this.pawn.mindState.duty.attackDownedIfStarving && ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002a: stateMachine*/)._0024this.pawn.Starving())
				{
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_002a: stateMachine*/)._0024this.job.killIncappedTarget = true;
				}
			});
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void Notify_PatherFailed()
		{
			if (job.attackDoorIfTargetLost)
			{
				Thing thing;
				using (PawnPath pawnPath = base.Map.pathFinder.FindPath(pawn.Position, base.TargetA.Cell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
				{
					if (!pawnPath.Found)
					{
						return;
					}
					thing = pawnPath.FirstBlockingBuilding(out IntVec3 _, pawn);
				}
				if (thing != null)
				{
					job.targetA = thing;
					job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
					job.expiryInterval = Rand.Range(2000, 4000);
					return;
				}
			}
			base.Notify_PatherFailed();
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
