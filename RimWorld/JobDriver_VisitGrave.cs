using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_VisitGrave : JobDriver_VisitJoyThing
	{
		private Building_Grave Grave => (Building_Grave)job.GetTarget(TargetIndex.A).Thing;

		protected override void WaitTickAction()
		{
			float num = 1f;
			Room room = base.pawn.GetRoom();
			if (room != null)
			{
				num *= room.GetStat(RoomStatDefOf.GraveVisitingJoyGainFactor);
			}
			base.pawn.GainComfortFromCellIfPossible();
			Pawn pawn = base.pawn;
			float extraJoyGainFactor = num;
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, Grave);
		}

		public override object[] TaleParameters()
		{
			return new object[2]
			{
				pawn,
				(Grave.Corpse == null) ? null : Grave.Corpse.InnerPawn
			};
		}
	}
}
