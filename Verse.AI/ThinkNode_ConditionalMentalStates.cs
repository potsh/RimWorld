using System.Collections.Generic;

namespace Verse.AI
{
	public class ThinkNode_ConditionalMentalStates : ThinkNode_Conditional
	{
		public List<MentalStateDef> states;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ConditionalMentalStates thinkNode_ConditionalMentalStates = (ThinkNode_ConditionalMentalStates)base.DeepCopy(resolve);
			thinkNode_ConditionalMentalStates.states = states;
			return thinkNode_ConditionalMentalStates;
		}

		protected override bool Satisfied(Pawn pawn)
		{
			return states.Contains(pawn.MentalStateDef);
		}
	}
}
