using Verse;

namespace RimWorld
{
	public class Thought_SituationalSocial : Thought_Situational, ISocialThought
	{
		public Pawn otherPawn;

		public override bool VisibleInNeedsTab => base.VisibleInNeedsTab && MoodOffset() != 0f;

		public Pawn OtherPawn()
		{
			return otherPawn;
		}

		public virtual float OpinionOffset()
		{
			return base.CurStage.baseOpinionOffset;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_SituationalSocial thought_SituationalSocial = other as Thought_SituationalSocial;
			if (thought_SituationalSocial == null)
			{
				return false;
			}
			return base.GroupsWith(other) && otherPawn == thought_SituationalSocial.otherPawn;
		}

		protected override ThoughtState CurrentStateInternal()
		{
			return def.Worker.CurrentSocialState(pawn, otherPawn);
		}
	}
}
