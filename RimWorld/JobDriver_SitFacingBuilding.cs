using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SitFacingBuilding : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo targetA = base.job.targetA;
			Job job = base.job;
			int joyMaxParticipants = base.job.def.joyMaxParticipants;
			int stackCount = 0;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(targetA, job, joyMaxParticipants, stackCount, null, errorOnFailed2))
			{
				pawn = base.pawn;
				targetA = base.job.targetB;
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
			this.EndOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		protected virtual void ModifyPlayToil(Toil toil)
		{
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
