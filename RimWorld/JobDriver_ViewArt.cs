using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ViewArt : JobDriver_VisitJoyThing
	{
		private Thing ArtThing => job.GetTarget(TargetIndex.A).Thing;

		protected override void WaitTickAction()
		{
			float num = ArtThing.GetStatValue(StatDefOf.Beauty) / ArtThing.def.GetStatValueAbstract(StatDefOf.Beauty);
			float num2 = (!(num > 0f)) ? 0f : num;
			base.pawn.GainComfortFromCellIfPossible();
			Pawn pawn = base.pawn;
			float extraJoyGainFactor = num2;
			JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, extraJoyGainFactor, (Building)ArtThing);
		}
	}
}
