namespace Verse
{
	public class HediffComp_Disappears : HediffComp
	{
		private int ticksToDisappear;

		public HediffCompProperties_Disappears Props => (HediffCompProperties_Disappears)props;

		public override bool CompShouldRemove => base.CompShouldRemove || ticksToDisappear <= 0;

		public override void CompPostMake()
		{
			base.CompPostMake();
			ticksToDisappear = Props.disappearsAfterTicks.RandomInRange;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToDisappear--;
		}

		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > ticksToDisappear)
			{
				ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToDisappear: " + ticksToDisappear;
		}
	}
}
