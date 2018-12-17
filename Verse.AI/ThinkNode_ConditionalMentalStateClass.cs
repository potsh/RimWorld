using System;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalStateClass : ThinkNode_Conditional
	{
		public Type stateClass;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalStateClass thinkNode_ConditionalMentalStateClass = (ThinkNode_ConditionalMentalStateClass)base.DeepCopy(resolve);
			thinkNode_ConditionalMentalStateClass.stateClass = stateClass;
			return thinkNode_ConditionalMentalStateClass;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			MentalState mentalState = pawn.MentalState;
			return mentalState != null && stateClass.IsAssignableFrom(mentalState.GetType());
		}
	}
}
