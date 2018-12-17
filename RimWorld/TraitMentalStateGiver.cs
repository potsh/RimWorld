using Verse;

namespace RimWorld
{
	public class TraitMentalStateGiver
	{
		public TraitDegreeData traitDegreeData;

		public virtual bool CheckGive(Pawn pawn, int checkInterval)
		{
			if (traitDegreeData.randomMentalState == null)
			{
				return false;
			}
			float curMood = pawn.mindState.mentalBreaker.CurMood;
			float mtb = traitDegreeData.randomMentalStateMtbDaysMoodCurve.Evaluate(curMood);
			if (Rand.MTBEventOccurs(mtb, 60000f, (float)checkInterval) && traitDegreeData.randomMentalState.Worker.StateCanOccur(pawn))
			{
				return pawn.mindState.mentalStateHandler.TryStartMentalState(traitDegreeData.randomMentalState, "MentalStateReason_Trait".Translate(traitDegreeData.label));
			}
			return false;
		}
	}
}
