using System;
using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker
	{
		public ThoughtDef def;

		public ThoughtState CurrentState(Pawn p)
		{
			return PostProcessedState(CurrentStateInternal(p));
		}

		public ThoughtState CurrentSocialState(Pawn p, Pawn otherPawn)
		{
			return PostProcessedState(CurrentSocialStateInternal(p, otherPawn));
		}

		private ThoughtState PostProcessedState(ThoughtState state)
		{
			if (def.invert)
			{
				state = ((!state.Active) ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive);
			}
			return state;
		}

		protected virtual ThoughtState CurrentStateInternal(Pawn p)
		{
			throw new NotImplementedException(def.defName + " (normal)");
		}

		protected virtual ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			throw new NotImplementedException(def.defName + " (social)");
		}
	}
}
