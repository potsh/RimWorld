using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Thought_MemorySocial : Thought_Memory, ISocialThought
	{
		public float opinionOffset;

		public override bool ShouldDiscard => otherPawn == null || opinionOffset == 0f || base.ShouldDiscard;

		public override bool VisibleInNeedsTab => base.VisibleInNeedsTab && MoodOffset() != 0f;

		private float AgePct => (float)age / (float)def.DurationTicks;

		private float AgeFactor => Mathf.InverseLerp(1f, def.lerpOpinionToZeroAfterDurationPct, AgePct);

		public virtual float OpinionOffset()
		{
			if (ShouldDiscard)
			{
				return 0f;
			}
			return opinionOffset * AgeFactor;
		}

		public Pawn OtherPawn()
		{
			return otherPawn;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref opinionOffset, "opinionOffset", 0f);
		}

		public override void Init()
		{
			base.Init();
			opinionOffset = base.CurStage.baseOpinionOffset;
		}

		public override bool TryMergeWithExistingMemory(out bool showBubble)
		{
			showBubble = false;
			return false;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_MemorySocial thought_MemorySocial = other as Thought_MemorySocial;
			if (thought_MemorySocial == null)
			{
				return false;
			}
			return base.GroupsWith(other) && otherPawn == thought_MemorySocial.otherPawn;
		}
	}
}
