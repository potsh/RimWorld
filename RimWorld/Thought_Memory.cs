using Verse;

namespace RimWorld
{
	public class Thought_Memory : Thought
	{
		public float moodPowerFactor = 1f;

		public Pawn otherPawn;

		public int age;

		private int forcedStage;

		private string cachedLabelCap;

		private Pawn cachedLabelCapForOtherPawn;

		private int cachedLabelCapForStageIndex = -1;

		public override bool VisibleInNeedsTab => base.VisibleInNeedsTab && !ShouldDiscard;

		public override int CurStageIndex => forcedStage;

		public virtual bool ShouldDiscard => age > def.DurationTicks;

		public override string LabelCap
		{
			get
			{
				if (cachedLabelCap == null || cachedLabelCapForOtherPawn != otherPawn || cachedLabelCapForStageIndex != CurStageIndex)
				{
					if (otherPawn != null)
					{
						cachedLabelCap = string.Format(base.CurStage.label, otherPawn.LabelShort).CapitalizeFirst();
					}
					else
					{
						cachedLabelCap = base.LabelCap;
					}
					cachedLabelCapForOtherPawn = otherPawn;
					cachedLabelCapForStageIndex = CurStageIndex;
				}
				return cachedLabelCap;
			}
		}

		public void SetForcedStage(int stageIndex)
		{
			forcedStage = stageIndex;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref otherPawn, "otherPawn", saveDestroyedThings: true);
			Scribe_Values.Look(ref moodPowerFactor, "moodPowerFactor", 1f);
			Scribe_Values.Look(ref age, "age", 0);
			Scribe_Values.Look(ref forcedStage, "stageIndex", 0);
		}

		public virtual void ThoughtInterval()
		{
			age += 150;
		}

		public void Renew()
		{
			age = 0;
		}

		public virtual bool TryMergeWithExistingMemory(out bool showBubble)
		{
			ThoughtHandler thoughts = pawn.needs.mood.thoughts;
			if (thoughts.memories.NumMemoriesInGroup(this) >= def.stackLimit)
			{
				Thought_Memory thought_Memory = thoughts.memories.OldestMemoryInGroup(this);
				if (thought_Memory != null)
				{
					showBubble = (thought_Memory.age > thought_Memory.def.DurationTicks / 2);
					thought_Memory.Renew();
					return true;
				}
			}
			showBubble = true;
			return false;
		}

		public override bool GroupsWith(Thought other)
		{
			Thought_Memory thought_Memory = other as Thought_Memory;
			if (thought_Memory == null)
			{
				return false;
			}
			return base.GroupsWith(other) && (otherPawn == thought_Memory.otherPawn || LabelCap == thought_Memory.LabelCap);
		}

		public override float MoodOffset()
		{
			float num = base.MoodOffset();
			return num * moodPowerFactor;
		}

		public override string ToString()
		{
			return "(" + def.defName + ", moodPowerFactor=" + moodPowerFactor + ", age=" + age + ")";
		}
	}
}
